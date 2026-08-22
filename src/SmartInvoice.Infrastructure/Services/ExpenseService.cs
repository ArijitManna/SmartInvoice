using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Expenses;
using SmartInvoice.Application.Expenses.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _context;

    public ExpenseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExpenseResponse>> CreateAsync(CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            CategoryId = request.CategoryId,
            VendorId = request.VendorId,
            Amount = request.Amount,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.Amount + request.TaxAmount,
            Date = request.Date ?? DateTime.UtcNow,
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber,
            PaymentMode = request.PaymentMode,
            Status = request.Status,
            IsRecurring = request.IsRecurring,
            RecurrenceFrequency = request.RecurrenceFrequency,
            ReceiptUrl = request.ReceiptUrl
        };

        if (expense.IsRecurring && expense.RecurrenceFrequency.HasValue)
        {
            expense.NextDueDate = CalculateNextDueDate(expense.Date, expense.RecurrenceFrequency.Value);
        }

        _context.Set<Expense>().Add(expense);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(expense.Id);
    }

    public async Task<PagedResult<ExpenseResponse>> GetAllAsync(int page = 1, int pageSize = 20, Guid? categoryId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<Expense>().Include(e => e.Category).Include(e => e.Vendor).AsQueryable();

        if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date < to.Value.Date.AddDays(1));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(e => e.Date).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ExpenseResponse>(items.Select(Map).ToList().AsReadOnly(), total, page, pageSize);
    }

    public async Task<Result<ExpenseResponse>> UpdateAsync(Guid id, CreateExpenseRequest request)
    {
        var expense = await _context.Set<Expense>().FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return Result<ExpenseResponse>.Failure("Expense not found.");

        expense.CategoryId = request.CategoryId;
        expense.VendorId = request.VendorId;
        expense.Amount = request.Amount;
        expense.TaxAmount = request.TaxAmount;
        expense.TotalAmount = request.Amount + request.TaxAmount;
        expense.Date = request.Date ?? expense.Date;
        expense.Description = request.Description;
        expense.ReferenceNumber = request.ReferenceNumber;
        expense.PaymentMode = request.PaymentMode;
        expense.Status = request.Status;
        expense.IsRecurring = request.IsRecurring;
        expense.RecurrenceFrequency = request.RecurrenceFrequency;
        expense.ReceiptUrl = request.ReceiptUrl;

        if (expense.IsRecurring && expense.RecurrenceFrequency.HasValue)
            expense.NextDueDate = CalculateNextDueDate(expense.Date, expense.RecurrenceFrequency.Value);

        await _context.SaveChangesAsync();
        return await GetByIdAsync(expense.Id);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var expense = await _context.Set<Expense>().FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return Result<bool>.Failure("Expense not found.");
        expense.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<ExpenseSummaryResponse> GetSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<Expense>().Include(e => e.Category).AsQueryable();
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date < to.Value.Date.AddDays(1));

        var expenses = await query.ToListAsync();

        var byCategory = expenses.GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new CategoryExpenseSummary(g.Key.CategoryId, g.Key.Name, g.Sum(e => e.TotalAmount), g.Count()))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new ExpenseSummaryResponse(expenses.Sum(e => e.TotalAmount), expenses.Count, byCategory);
    }

    // --- Categories ---

    public async Task<Result<ExpenseCategoryResponse>> CreateCategoryAsync(ExpenseCategoryRequest request)
    {
        var category = new ExpenseCategory { Name = request.Name, Description = request.Description, ParentId = request.ParentId };
        _context.Set<ExpenseCategory>().Add(category);
        await _context.SaveChangesAsync();

        string? parentName = null;
        if (request.ParentId.HasValue)
        {
            var parent = await _context.Set<ExpenseCategory>().FirstOrDefaultAsync(c => c.Id == request.ParentId.Value);
            parentName = parent?.Name;
        }

        return Result<ExpenseCategoryResponse>.Success(new(category.Id, category.Name, category.Description, category.ParentId, parentName));
    }

    public async Task<IReadOnlyList<ExpenseCategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _context.Set<ExpenseCategory>().Include(c => c.Parent).OrderBy(c => c.Name).ToListAsync();
        return categories.Select(c => new ExpenseCategoryResponse(c.Id, c.Name, c.Description, c.ParentId, c.Parent?.Name)).ToList().AsReadOnly();
    }

    public async Task<Result<bool>> DeleteCategoryAsync(Guid id)
    {
        var cat = await _context.Set<ExpenseCategory>().FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null) return Result<bool>.Failure("Category not found.");
        cat.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // --- Helpers ---

    private async Task<Result<ExpenseResponse>> GetByIdAsync(Guid id)
    {
        var expense = await _context.Set<Expense>().Include(e => e.Category).Include(e => e.Vendor).FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null) return Result<ExpenseResponse>.Failure("Expense not found.");
        return Result<ExpenseResponse>.Success(Map(expense));
    }

    private static ExpenseResponse Map(Expense e) => new(
        e.Id, e.CategoryId, e.Category.Name, e.VendorId, e.Vendor?.Name,
        e.Amount, e.TaxAmount, e.TotalAmount, e.Date, e.Description,
        e.ReferenceNumber, e.PaymentMode, e.Status,
        e.IsRecurring, e.RecurrenceFrequency, e.NextDueDate, e.ReceiptUrl, e.CreatedAt
    );

    private static DateTime CalculateNextDueDate(DateTime from, Domain.Enums.RecurrenceFrequency freq) => freq switch
    {
        Domain.Enums.RecurrenceFrequency.Daily => from.AddDays(1),
        Domain.Enums.RecurrenceFrequency.Weekly => from.AddDays(7),
        Domain.Enums.RecurrenceFrequency.Monthly => from.AddMonths(1),
        Domain.Enums.RecurrenceFrequency.Quarterly => from.AddMonths(3),
        Domain.Enums.RecurrenceFrequency.Yearly => from.AddYears(1),
        _ => from.AddMonths(1)
    };
}
