using SmartInvoice.Application.Reports.DTOs;

namespace SmartInvoice.Application.Reports;

public interface IReportService
{
    Task<SalesReportResponse> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId = null);
    Task<OutstandingReportResponse> GetOutstandingReportAsync();
    Task<GstReportResponse> GetGstReportAsync(DateTime from, DateTime to);
}
