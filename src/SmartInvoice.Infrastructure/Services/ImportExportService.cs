using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.ImportExport;
using SmartInvoice.Application.ImportExport.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Domain.ValueObjects;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class ImportExportService : IImportExportService
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string CsvContentType = "text/csv";

    private readonly AppDbContext _context;

    public ImportExportService(AppDbContext context)
    {
        _context = context;
    }

    // ---------------------------------------------------------------------
    // IMPORTS
    // ---------------------------------------------------------------------

    public async Task<ImportResult> ImportProductsAsync(Stream fileStream, string fileName)
    {
        var rows = ReadRows(fileStream, fileName, out var headers);
        var errors = new List<ImportRowError>();
        var toAdd = new List<Product>();
        int success = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2; // account for header row
            var row = rows[i];
            try
            {
                var name = Get(row, headers, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(new ImportRowError(rowNum, "Name is required."));
                    continue;
                }

                var product = new Product
                {
                    Name = name.Trim(),
                    Description = Get(row, headers, "Description"),
                    Sku = Get(row, headers, "SKU"),
                    HsnSacCode = Get(row, headers, "HSN/SAC"),
                    Unit = string.IsNullOrWhiteSpace(Get(row, headers, "Unit")) ? "Nos" : Get(row, headers, "Unit")!.Trim(),
                    Price = ParseDecimal(Get(row, headers, "Selling Price")),
                    PurchasePrice = ParseDecimal(Get(row, headers, "Purchase Price")),
                    TaxRate = ParseDecimal(Get(row, headers, "Tax Rate")),
                    OpeningStock = ParseDecimal(Get(row, headers, "Opening Stock")),
                    LowStockThreshold = (int)ParseDecimal(Get(row, headers, "Low Stock Alert")),
                    Barcode = Get(row, headers, "Barcode"),
                    Brand = Get(row, headers, "Brand"),
                    TrackInventory = ParseBool(Get(row, headers, "Track Inventory"))
                };
                toAdd.Add(product);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(rowNum, ex.Message));
            }
        }

        if (toAdd.Count > 0)
        {
            _context.Products.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }

        return await FinalizeAsync(ImportType.Products, fileName, rows.Count, success, errors);
    }

    public async Task<ImportResult> ImportCustomersAsync(Stream fileStream, string fileName)
    {
        var rows = ReadRows(fileStream, fileName, out var headers);
        var errors = new List<ImportRowError>();
        var toAdd = new List<Customer>();
        int success = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            var row = rows[i];
            try
            {
                var name = Get(row, headers, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(new ImportRowError(rowNum, "Name is required."));
                    continue;
                }

                var customer = new Customer
                {
                    Name = name.Trim(),
                    Email = Get(row, headers, "Email"),
                    Phone = Get(row, headers, "Phone"),
                    ContactPerson = Get(row, headers, "Contact Person"),
                    Notes = Get(row, headers, "Notes"),
                    CreditLimit = ParseDecimal(Get(row, headers, "Credit Limit")),
                    OpeningBalance = ParseDecimal(Get(row, headers, "Opening Balance")),
                    GstInfo = new GstInfo
                    {
                        Gstin = Get(row, headers, "GSTIN"),
                        Pan = Get(row, headers, "PAN"),
                        StateCode = Get(row, headers, "State Code")
                    },
                    BillingAddress = new Address
                    {
                        Street = Get(row, headers, "Street") ?? string.Empty,
                        City = Get(row, headers, "City") ?? string.Empty,
                        State = Get(row, headers, "State") ?? string.Empty,
                        PostalCode = Get(row, headers, "Postal Code") ?? string.Empty,
                        Country = string.IsNullOrWhiteSpace(Get(row, headers, "Country")) ? "India" : Get(row, headers, "Country")!.Trim()
                    }
                };
                toAdd.Add(customer);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(rowNum, ex.Message));
            }
        }

        if (toAdd.Count > 0)
        {
            _context.Customers.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }

        return await FinalizeAsync(ImportType.Customers, fileName, rows.Count, success, errors);
    }

    public async Task<ImportResult> ImportVendorsAsync(Stream fileStream, string fileName)
    {
        var rows = ReadRows(fileStream, fileName, out var headers);
        var errors = new List<ImportRowError>();
        var toAdd = new List<Vendor>();
        int success = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            var row = rows[i];
            try
            {
                var name = Get(row, headers, "Name");
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add(new ImportRowError(rowNum, "Name is required."));
                    continue;
                }

                var vendor = new Vendor
                {
                    Name = name.Trim(),
                    Email = Get(row, headers, "Email"),
                    Phone = Get(row, headers, "Phone"),
                    ContactPerson = Get(row, headers, "Contact Person"),
                    Notes = Get(row, headers, "Notes"),
                    OpeningBalance = ParseDecimal(Get(row, headers, "Opening Balance")),
                    GstInfo = new GstInfo
                    {
                        Gstin = Get(row, headers, "GSTIN"),
                        Pan = Get(row, headers, "PAN"),
                        StateCode = Get(row, headers, "State Code")
                    },
                    Address = new Address
                    {
                        Street = Get(row, headers, "Street") ?? string.Empty,
                        City = Get(row, headers, "City") ?? string.Empty,
                        State = Get(row, headers, "State") ?? string.Empty,
                        PostalCode = Get(row, headers, "Postal Code") ?? string.Empty,
                        Country = string.IsNullOrWhiteSpace(Get(row, headers, "Country")) ? "India" : Get(row, headers, "Country")!.Trim()
                    }
                };
                toAdd.Add(vendor);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(rowNum, ex.Message));
            }
        }

        if (toAdd.Count > 0)
        {
            _context.Vendors.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }

        return await FinalizeAsync(ImportType.Vendors, fileName, rows.Count, success, errors);
    }

    public async Task<ImportResult> ImportOpeningStockAsync(Stream fileStream, string fileName)
    {
        var rows = ReadRows(fileStream, fileName, out var headers);
        var errors = new List<ImportRowError>();
        var toAdd = new List<StockEntry>();
        int success = 0;

        var defaultWarehouse = await _context.Warehouses
            .OrderByDescending(w => w.IsDefault)
            .FirstOrDefaultAsync();

        if (defaultWarehouse == null)
        {
            return await FinalizeAsync(ImportType.OpeningStock, fileName, rows.Count, 0,
                new List<ImportRowError> { new(0, "No warehouse found. Create a warehouse before importing opening stock.") });
        }

        // Preload products (by SKU and by Name) for matching
        var products = await _context.Products.ToListAsync();
        var bySku = products.Where(p => !string.IsNullOrWhiteSpace(p.Sku))
            .GroupBy(p => p.Sku!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());
        var byName = products
            .GroupBy(p => p.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        for (int i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            var row = rows[i];
            try
            {
                var sku = Get(row, headers, "SKU");
                var name = Get(row, headers, "Product Name");
                Product? product = null;

                if (!string.IsNullOrWhiteSpace(sku) && bySku.TryGetValue(sku.Trim().ToLowerInvariant(), out var pBySku))
                    product = pBySku;
                else if (!string.IsNullOrWhiteSpace(name) && byName.TryGetValue(name.Trim().ToLowerInvariant(), out var pByName))
                    product = pByName;

                if (product == null)
                {
                    errors.Add(new ImportRowError(rowNum, $"Product not found (SKU='{sku}', Name='{name}')."));
                    continue;
                }

                var qty = ParseDecimal(Get(row, headers, "Quantity"));
                toAdd.Add(new StockEntry
                {
                    ProductId = product.Id,
                    WarehouseId = defaultWarehouse.Id,
                    Quantity = qty,
                    Type = StockEntryType.Opening,
                    ReferenceType = "OpeningStockImport",
                    Notes = "Imported opening stock",
                    Date = DateTime.UtcNow
                });
                success++;
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(rowNum, ex.Message));
            }
        }

        if (toAdd.Count > 0)
        {
            _context.StockEntries.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }

        return await FinalizeAsync(ImportType.OpeningStock, fileName, rows.Count, success, errors);
    }

    // ---------------------------------------------------------------------
    // EXPORTS
    // ---------------------------------------------------------------------

    public async Task<ExportFile> ExportProductsAsync(ExportFormat format)
    {
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        var headers = new[] { "Name", "Description", "SKU", "HSN/SAC", "Unit", "Selling Price", "Purchase Price", "Tax Rate", "Opening Stock", "Low Stock Alert", "Barcode", "Brand", "Track Inventory" };
        var data = products.Select(p => new object?[]
        {
            p.Name, p.Description, p.Sku, p.HsnSacCode, p.Unit, p.Price, p.PurchasePrice, p.TaxRate,
            p.OpeningStock, p.LowStockThreshold, p.Barcode, p.Brand, p.TrackInventory ? "Yes" : "No"
        }).ToList();

        return Build(format, "Products", headers, data);
    }

    public async Task<ExportFile> ExportCustomersAsync(ExportFormat format)
    {
        var customers = await _context.Customers.OrderBy(c => c.Name).ToListAsync();
        var headers = new[] { "Name", "Email", "Phone", "Contact Person", "GSTIN", "PAN", "State Code", "Street", "City", "State", "Postal Code", "Country", "Credit Limit", "Opening Balance", "Outstanding Balance", "Notes" };
        var data = customers.Select(c => new object?[]
        {
            c.Name, c.Email, c.Phone, c.ContactPerson, c.GstInfo.Gstin, c.GstInfo.Pan, c.GstInfo.StateCode,
            c.BillingAddress.Street, c.BillingAddress.City, c.BillingAddress.State, c.BillingAddress.PostalCode, c.BillingAddress.Country,
            c.CreditLimit, c.OpeningBalance, c.OutstandingBalance, c.Notes
        }).ToList();

        return Build(format, "Customers", headers, data);
    }

    public async Task<ExportFile> ExportVendorsAsync(ExportFormat format)
    {
        var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync();
        var headers = new[] { "Name", "Email", "Phone", "Contact Person", "GSTIN", "PAN", "State Code", "Street", "City", "State", "Postal Code", "Country", "Opening Balance", "Outstanding Balance", "Notes" };
        var data = vendors.Select(v => new object?[]
        {
            v.Name, v.Email, v.Phone, v.ContactPerson, v.GstInfo.Gstin, v.GstInfo.Pan, v.GstInfo.StateCode,
            v.Address.Street, v.Address.City, v.Address.State, v.Address.PostalCode, v.Address.Country,
            v.OpeningBalance, v.OutstandingBalance, v.Notes
        }).ToList();

        return Build(format, "Vendors", headers, data);
    }

    public async Task<ExportFile> ExportInvoicesAsync(ExportFormat format)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
        var headers = new[] { "Invoice Number", "Date", "Due Date", "Customer", "Status", "Sub Total", "Discount", "Tax", "Total", "Paid", "Balance" };
        var data = invoices.Select(i => new object?[]
        {
            i.InvoiceNumber, i.InvoiceDate.ToString("yyyy-MM-dd"), i.DueDate.ToString("yyyy-MM-dd"),
            i.Customer.Name, i.Status.ToString(), i.SubTotal, i.DiscountAmount, i.TaxAmount, i.TotalAmount,
            i.PaidAmount, i.TotalAmount - i.PaidAmount
        }).ToList();

        return Build(format, "Invoices", headers, data);
    }

    // ---------------------------------------------------------------------
    // TEMPLATES
    // ---------------------------------------------------------------------

    public ExportFile GetProductTemplate() =>
        BuildTemplate("Products", new[] { "Name", "Description", "SKU", "HSN/SAC", "Unit", "Selling Price", "Purchase Price", "Tax Rate", "Opening Stock", "Low Stock Alert", "Barcode", "Brand", "Track Inventory" });

    public ExportFile GetCustomerTemplate() =>
        BuildTemplate("Customers", new[] { "Name", "Email", "Phone", "Contact Person", "GSTIN", "PAN", "State Code", "Street", "City", "State", "Postal Code", "Country", "Credit Limit", "Opening Balance", "Notes" });

    public ExportFile GetVendorTemplate() =>
        BuildTemplate("Vendors", new[] { "Name", "Email", "Phone", "Contact Person", "GSTIN", "PAN", "State Code", "Street", "City", "State", "Postal Code", "Country", "Opening Balance", "Notes" });

    public ExportFile GetOpeningStockTemplate() =>
        BuildTemplate("OpeningStock", new[] { "SKU", "Product Name", "Quantity" });

    // ---------------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------------

    private async Task<ImportResult> FinalizeAsync(ImportType type, string fileName, int total, int success, List<ImportRowError> errors)
    {
        var job = new ImportJob
        {
            Type = type,
            FileName = fileName,
            Status = errors.Count == 0 ? ImportStatus.Completed : (success == 0 ? ImportStatus.Failed : ImportStatus.Completed),
            TotalRows = total,
            SuccessRows = success,
            ErrorRows = errors.Count,
            ErrorLog = errors.Count == 0 ? null : string.Join("\n", errors.Select(e => $"Row {e.RowNumber}: {e.Message}")),
            CompletedAt = DateTime.UtcNow
        };
        _context.ImportJobs.Add(job);
        await _context.SaveChangesAsync();

        return new ImportResult(type, total, success, errors.Count, errors);
    }

    /// <summary>Reads all data rows into a list of string arrays, returns headers separately.</summary>
    private static List<string?[]> ReadRows(Stream fileStream, string fileName, out List<string> headers)
    {
        if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return ReadCsvRows(fileStream, out headers);
        return ReadExcelRows(fileStream, out headers);
    }

    private static List<string?[]> ReadExcelRows(Stream fileStream, out List<string> headers)
    {
        headers = new List<string>();
        var rows = new List<string?[]>();

        using var workbook = new XLWorkbook(fileStream);
        var sheet = workbook.Worksheets.FirstOrDefault();
        if (sheet == null) return rows;

        var range = sheet.RangeUsed();
        if (range == null) return rows;

        var allRows = range.RowsUsed().ToList();
        if (allRows.Count == 0) return rows;

        // Header row
        var headerCells = allRows[0].Cells().ToList();
        foreach (var cell in headerCells)
            headers.Add(cell.GetString().Trim());

        for (int r = 1; r < allRows.Count; r++)
        {
            var cells = allRows[r].Cells(1, headers.Count).ToList();
            var values = new string?[headers.Count];
            for (int c = 0; c < headers.Count && c < cells.Count; c++)
                values[c] = cells[c].GetString();
            // Skip fully empty rows
            if (values.All(v => string.IsNullOrWhiteSpace(v))) continue;
            rows.Add(values);
        }

        return rows;
    }

    private static List<string?[]> ReadCsvRows(Stream fileStream, out List<string> headers)
    {
        headers = new List<string>();
        var rows = new List<string?[]>();

        using var reader = new StreamReader(fileStream);
        string? line;
        bool first = true;
        while ((line = reader.ReadLine()) != null)
        {
            var fields = ParseCsvLine(line);
            if (first)
            {
                headers = fields.Select(f => f.Trim()).ToList();
                first = false;
                continue;
            }
            if (fields.All(string.IsNullOrWhiteSpace)) continue;
            var values = new string?[headers.Count];
            for (int c = 0; c < headers.Count && c < fields.Count; c++)
                values[c] = fields[c];
            rows.Add(values);
        }

        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(ch);
            }
            else
            {
                if (ch == '"') inQuotes = true;
                else if (ch == ',') { result.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(ch);
            }
        }
        result.Add(sb.ToString());
        return result;
    }

    private static string? Get(string?[] row, List<string> headers, string columnName)
    {
        var idx = headers.FindIndex(h => string.Equals(h, columnName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0 || idx >= row.Length) return null;
        var val = row[idx];
        return string.IsNullOrWhiteSpace(val) ? null : val.Trim();
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        return decimal.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            || decimal.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out d)
            ? d : 0;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        value = value.Trim().ToLowerInvariant();
        return value is "yes" or "true" or "1" or "y";
    }

    private static ExportFile Build(ExportFormat format, string name, string[] headers, List<object?[]> data)
    {
        return format == ExportFormat.Csv
            ? BuildCsv(name, headers, data)
            : BuildExcel(name, headers, data);
    }

    private static ExportFile BuildExcel(string name, string[] headers, List<object?[]> data)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(name);

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
        }

        for (int r = 0; r < data.Count; r++)
        {
            for (int c = 0; c < headers.Length && c < data[r].Length; c++)
            {
                var value = data[r][c];
                var cell = sheet.Cell(r + 2, c + 1);
                switch (value)
                {
                    case null: break;
                    case decimal dec: cell.Value = dec; break;
                    case int intVal: cell.Value = intVal; break;
                    case bool b: cell.Value = b; break;
                    default: cell.Value = value.ToString(); break;
                }
            }
        }

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return new ExportFile(ms.ToArray(), $"{name}_{DateTime.UtcNow:yyyyMMdd}.xlsx", ExcelContentType);
    }

    private static ExportFile BuildCsv(string name, string[] headers, List<object?[]> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
        foreach (var row in data)
        {
            var fields = new string[headers.Length];
            for (int c = 0; c < headers.Length; c++)
                fields[c] = EscapeCsv(c < row.Length ? row[c]?.ToString() ?? string.Empty : string.Empty);
            sb.AppendLine(string.Join(",", fields));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new ExportFile(bytes, $"{name}_{DateTime.UtcNow:yyyyMMdd}.csv", CsvContentType);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static ExportFile BuildTemplate(string name, string[] headers)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(name);
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
        }
        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return new ExportFile(ms.ToArray(), $"{name}_Template.xlsx", ExcelContentType);
    }
}
