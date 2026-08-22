using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Application.Expenses.DTOs;

public record CreateExpenseRequest(
    Guid CategoryId,
    Guid? VendorId,
    decimal Amount,
    decimal TaxAmount,
    DateTime? Date,
    string Description,
    string? ReferenceNumber,
    PaymentMode PaymentMode,
    ExpenseStatus Status,
    bool IsRecurring,
    RecurrenceFrequency? RecurrenceFrequency,
    string? ReceiptUrl
);

public record ExpenseResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid? VendorId,
    string? VendorName,
    decimal Amount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTime Date,
    string Description,
    string? ReferenceNumber,
    PaymentMode PaymentMode,
    ExpenseStatus Status,
    bool IsRecurring,
    RecurrenceFrequency? RecurrenceFrequency,
    DateTime? NextDueDate,
    string? ReceiptUrl,
    DateTime CreatedAt
);

public record ExpenseCategoryRequest(string Name, string? Description, Guid? ParentId);

public record ExpenseCategoryResponse(Guid Id, string Name, string? Description, Guid? ParentId, string? ParentName);

public record ExpenseSummaryResponse(
    decimal TotalExpenses,
    int TotalCount,
    List<CategoryExpenseSummary> ByCategory
);

public record CategoryExpenseSummary(Guid CategoryId, string CategoryName, decimal Total, int Count);
