using SmartInvoice.Application.Common;
using SmartInvoice.Application.Payments.DTOs;

namespace SmartInvoice.Application.Payments;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequest request);
    Task<IReadOnlyList<PaymentResponse>> GetPaymentsByInvoiceAsync(Guid invoiceId);
    Task<Result<PaymentResponse>> RefundAsync(Guid paymentId, string? notes);
}
