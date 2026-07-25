namespace SmartInvoice.Application.Email;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(Guid invoiceId, string recipientEmail);
}
