using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Payments;
using SmartInvoice.Application.Payments.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaymentResponse>> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequest request)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            return Result<PaymentResponse>.Failure("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<PaymentResponse>.Failure("Cannot record payment on a cancelled invoice.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<PaymentResponse>.Failure("Invoice is already fully paid.");
        }

        if (request.Amount <= 0)
        {
            return Result<PaymentResponse>.Failure("Payment amount must be positive.");
        }

        if (request.Amount > invoice.BalanceDue)
        {
            return Result<PaymentResponse>.Failure($"Payment amount ({request.Amount:N2}) exceeds balance due ({invoice.BalanceDue:N2}).");
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            Amount = request.Amount,
            PaymentMode = request.PaymentMode,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            IsRefund = false,
            CompanyId = invoice.CompanyId
        };

        _context.Payments.Add(payment);

        // Update invoice
        invoice.RecordPayment(request.Amount);

        await _context.SaveChangesAsync();

        return Result<PaymentResponse>.Success(MapToResponse(payment));
    }

    public async Task<IReadOnlyList<PaymentResponse>> GetPaymentsByInvoiceAsync(Guid invoiceId)
    {
        var payments = await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<Result<PaymentResponse>> RefundAsync(Guid paymentId, string? notes)
    {
        var originalPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (originalPayment is null)
        {
            return Result<PaymentResponse>.Failure("Payment not found.");
        }

        if (originalPayment.IsRefund)
        {
            return Result<PaymentResponse>.Failure("Cannot refund a refund.");
        }

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == originalPayment.InvoiceId);
        if (invoice is null)
        {
            return Result<PaymentResponse>.Failure("Invoice not found.");
        }

        // Create refund payment record
        var refund = new Payment
        {
            InvoiceId = originalPayment.InvoiceId,
            Amount = originalPayment.Amount,
            PaymentMode = originalPayment.PaymentMode,
            PaymentDate = DateTime.UtcNow,
            ReferenceNumber = $"REFUND-{originalPayment.ReferenceNumber}",
            Notes = notes ?? $"Refund of payment {originalPayment.Id}",
            IsRefund = true,
            CompanyId = originalPayment.CompanyId
        };

        _context.Payments.Add(refund);

        // Update invoice amounts
        invoice.AmountPaid -= originalPayment.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;

        if (invoice.AmountPaid <= 0)
        {
            invoice.Status = InvoiceStatus.Sent;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync();

        return Result<PaymentResponse>.Success(MapToResponse(refund));
    }

    private static PaymentResponse MapToResponse(Payment p)
    {
        return new PaymentResponse(
            Id: p.Id,
            InvoiceId: p.InvoiceId,
            Amount: p.Amount,
            PaymentMode: p.PaymentMode,
            PaymentDate: p.PaymentDate,
            ReferenceNumber: p.ReferenceNumber,
            Notes: p.Notes,
            IsRefund: p.IsRefund,
            CreatedAt: p.CreatedAt
        );
    }
}
