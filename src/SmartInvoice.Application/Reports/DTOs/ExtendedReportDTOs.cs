namespace SmartInvoice.Application.Reports.DTOs;

// HSN Summary
public record HsnSummaryResponse(decimal TotalTaxableValue, decimal TotalTax, List<HsnSummaryItem> Items);
public record HsnSummaryItem(string HsnCode, string Description, int Quantity, decimal TaxableValue, decimal CgstAmount, decimal SgstAmount, decimal IgstAmount, decimal TotalTax);

// GSTR-1
public record Gstr1Response(List<Gstr1B2BEntry> B2B, List<Gstr1B2CEntry> B2C, decimal TotalTaxableB2B, decimal TotalTaxableB2C, decimal TotalTax);
public record Gstr1B2BEntry(string CustomerGstin, string CustomerName, string InvoiceNumber, DateTime InvoiceDate, decimal TaxableValue, decimal CgstAmount, decimal SgstAmount, decimal IgstAmount, decimal TotalTax);
public record Gstr1B2CEntry(string InvoiceNumber, DateTime InvoiceDate, decimal TaxableValue, decimal TaxAmount);

// Profit & Loss
public record ProfitLossResponse(decimal TotalRevenue, decimal CostOfGoodsSold, decimal GrossProfit, decimal TotalExpenses, decimal NetProfit, List<ProfitLossLineItem> RevenueBreakdown, List<ProfitLossLineItem> ExpenseBreakdown);
public record ProfitLossLineItem(string Category, decimal Amount);

// Cash Flow
public record CashFlowResponse(decimal TotalInflows, decimal TotalOutflows, decimal NetCashFlow, List<CashFlowPeriod> Periods);
public record CashFlowPeriod(string Period, decimal Inflows, decimal Outflows, decimal Net);

// Product Report
public record ProductReportResponse(List<ProductSalesItem> TopSelling, List<ProductSalesItem> SlowMoving, decimal TotalProductRevenue);
public record ProductSalesItem(Guid ProductId, string ProductName, string? Sku, int QuantitySold, decimal Revenue);

// Yearly Revenue
public record YearlyRevenueResponse(int Year, decimal Total, List<MonthlyAmount> Months);
public record MonthlyAmount(int Month, string MonthName, decimal Amount);

// Tax Collected
public record TaxCollectedResponse(decimal TotalTax, decimal TotalCgst, decimal TotalSgst, decimal TotalIgst, List<TaxPeriodItem> ByPeriod);
public record TaxPeriodItem(string Period, decimal Cgst, decimal Sgst, decimal Igst, decimal Total);
