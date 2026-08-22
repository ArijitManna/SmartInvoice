using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Expenses;
using SmartInvoice.Application.Expenses.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    [RequirePermission("Expense.Create")]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.CreateAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpGet]
    [RequirePermission("Expense.View")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? categoryId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _expenseService.GetAllAsync(page, pageSize, categoryId, from, to);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Expense.Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.UpdateAsync(id, request);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Expense.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _expenseService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return NoContent();
    }

    [HttpGet("summary")]
    [RequirePermission("Expense.View")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var summary = await _expenseService.GetSummaryAsync(from, to);
        return Ok(summary);
    }

    // --- Categories ---

    [HttpPost("categories")]
    [RequirePermission("Expense.Create")]
    public async Task<IActionResult> CreateCategory([FromBody] ExpenseCategoryRequest request)
    {
        var result = await _expenseService.CreateCategoryAsync(request);
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        return Created("", result.Value);
    }

    [HttpGet("categories")]
    [RequirePermission("Expense.View")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _expenseService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpDelete("categories/{id:guid}")]
    [RequirePermission("Expense.Delete")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _expenseService.DeleteCategoryAsync(id);
        if (!result.IsSuccess) return NotFound(new { Error = result.Error });
        return NoContent();
    }
}
