namespace SmartInvoice.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Cancelled = 5,
    Viewed = 6,
    Accepted = 7
}
