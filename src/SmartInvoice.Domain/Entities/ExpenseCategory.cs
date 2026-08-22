using SmartInvoice.Domain.Common;

namespace SmartInvoice.Domain.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsDefault { get; set; }

    // Navigation
    public ExpenseCategory? Parent { get; set; }
    public ICollection<ExpenseCategory> Children { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
}
