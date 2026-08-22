namespace SmartInvoice.Domain.Enums;

public enum LedgerEntryType
{
    Opening = 0,
    Invoice = 1,
    Payment = 2,
    CreditNote = 3,
    DebitNote = 4,
    Refund = 5,
    Adjustment = 6
}
