namespace SmartInvoice.Domain.Enums;

public enum ImportType
{
    Products = 0,
    Customers = 1,
    Vendors = 2,
    OpeningStock = 3
}

public enum ImportStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
