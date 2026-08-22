using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInvoice.API.Filters;
using SmartInvoice.Application.Products;
using SmartInvoice.Application.Products.DTOs;

namespace SmartInvoice.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [RequirePermission("Product.Create")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var result = await _productService.GetAllAsync(page, pageSize, search, categoryId, sortBy, sortDesc);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Product.Edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Product.Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            return NotFound(new { Error = result.Error });
        }

        return NoContent();
    }
}
