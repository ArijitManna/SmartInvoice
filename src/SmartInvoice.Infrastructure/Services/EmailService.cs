using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartInvoice.Application.Email;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly AppDbContext _context;
    private readonly IInvoicePdfService _pdfService;
    private readonly EmailSettings _settings;

    public EmailService(AppDbContext context, IInvoicePdfService pdfService, IOptions<EmailSettings> settings)
    {
        _context = context;
        _pdfService = pdfService;
        _settings = settings.Value;
    }

    public async Task SendInvoiceEmailAsync(Guid invoiceId, string recipientEmail)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found.");
        }

        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == invoice.CompanyId && !c.IsDeleted);

        // Generate PDF
        var pdfBytes = await _pdfService.GenerateAsync(invoiceId);

        // Build email
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(invoice.Customer.Name, recipientEmail));
        message.Subject = $"Invoice {invoice.InvoiceNumber} from {company.Name}";

        var builder = new BodyBuilder
        {
            HtmlBody = BuildEmailHtml(invoice, company)
        };

        builder.Attachments.Add($"{invoice.InvoiceNumber}.pdf", pdfBytes, new ContentType("application", "pdf"));

        message.Body = builder.ToMessageBody();

        // Send via SMTP
        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, _settings.UseSsl);

        if (!string.IsNullOrEmpty(_settings.SmtpUsername))
        {
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword ?? string.Empty);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        // Update invoice status to Sent if it was Draft
        if (invoice.Status == InvoiceStatus.Draft)
        {
            invoice.Status = InvoiceStatus.Sent;
            await _context.SaveChangesAsync();
        }
    }

    private static string BuildEmailHtml(Domain.Entities.Invoice invoice, Domain.Entities.Company company)
    {
        return $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #1a56db;">{company.Name}</h2>
                <p>Dear {invoice.Customer.Name},</p>
                <p>Please find attached invoice <strong>{invoice.InvoiceNumber}</strong>.</p>
                <table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                    <tr style="background: #f3f4f6;">
                        <td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Invoice #</strong></td>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;">{invoice.InvoiceNumber}</td>
                    </tr>
                    <tr>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Date</strong></td>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;">{invoice.InvoiceDate:dd MMM yyyy}</td>
                    </tr>
                    <tr style="background: #f3f4f6;">
                        <td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Amount</strong></td>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;">{invoice.Currency} {invoice.TotalAmount:N2}</td>
                    </tr>
                    <tr>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;"><strong>Due Date</strong></td>
                        <td style="padding: 8px; border: 1px solid #e5e7eb;">{(invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("dd MMM yyyy") : "On Receipt")}</td>
                    </tr>
                </table>
                <p>Please make the payment at your earliest convenience.</p>
                <p>Thank you for your business!</p>
                <hr style="border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;" />
                <p style="font-size: 12px; color: #6b7280;">{company.Name} | {company.Email} | {company.Phone}</p>
            </div>
            """;
    }
}
