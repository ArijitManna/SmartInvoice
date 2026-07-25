namespace SmartInvoice.Application.Invoices;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(Guid invoiceId);
}
