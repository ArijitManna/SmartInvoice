using SmartInvoice.Application.Reports.DTOs;

namespace SmartInvoice.Application.Reports;

public interface IExtendedReportService
{
    Task<HsnSummaryResponse> GetHsnSummaryAsync(DateTime from, DateTime to);
    Task<Gstr1Response> GetGstr1ReportAsync(DateTime from, DateTime to);
    Task<ProfitLossResponse> GetProfitLossAsync(DateTime from, DateTime to);
    Task<CashFlowResponse> GetCashFlowAsync(DateTime from, DateTime to);
    Task<ProductReportResponse> GetProductReportAsync(DateTime from, DateTime to);
    Task<YearlyRevenueResponse> GetYearlyRevenueAsync(int year);
    Task<TaxCollectedResponse> GetTaxCollectedAsync(DateTime from, DateTime to);
}
