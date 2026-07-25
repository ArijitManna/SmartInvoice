namespace SmartInvoice.Domain.Enums;

public enum InvoiceType
{
    Regular = 0,
    GstInvoice = 1,
    Proforma = 2,
    CreditNote = 3,
    DebitNote = 4,
    Recurring = 5,
    Quotation = 6,
    Estimate = 7,
    Purchase = 8
}
