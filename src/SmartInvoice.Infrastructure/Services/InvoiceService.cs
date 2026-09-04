using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Application.Invoices.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyService _companyService;

    public InvoiceService(AppDbContext context, ICurrentCompanyService companyService)
    {
        _context = context;
        _companyService = companyService;
    }

    public async Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request)
    {
        try
        {
            // Load company for invoice number and state
            var companyId = _context.CurrentCompanyId;
            if (!companyId.HasValue)
            {
                return Result<InvoiceResponse>.Failure("No company context.");
            }

            var company = await _context.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId.Value && !c.IsDeleted);

            if (company is null)
            {
                return Result<InvoiceResponse>.Failure("Company not found.");
            }

            // Load customer for state comparison
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
            if (customer is null)
            {
                return Result<InvoiceResponse>.Failure("Customer not found.");
            }

            // Generate invoice number
            string invoiceNumber = company.GenerateInvoiceNumber();

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                Type = request.Type,
                Status = InvoiceStatus.Draft,
                InvoiceDate = DateTime.UtcNow,
                DueDate = request.DueDate.HasValue
                    ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc)
                    : null,
                CustomerId = request.CustomerId,
                DiscountPercentage = request.DiscountPercentage,
                Notes = request.Notes,
                TermsAndConditions = request.TermsAndConditions,
                ReferenceNumber = request.ReferenceNumber,
                Currency = company.DefaultCurrency,
                CompanyId = companyId.Value
            };

            // Create line items
            foreach (var itemReq in request.Items)
            {
                var item = new InvoiceItem
                {
                    ProductId = itemReq.ProductId,
                    Description = itemReq.Description,
                    HsnSacCode = itemReq.HsnSacCode ?? string.Empty,
                    Quantity = itemReq.Quantity,
                    Unit = itemReq.Unit ?? "Nos",
                    Rate = itemReq.Rate,
                    DiscountPercentage = itemReq.DiscountPercentage,
                    TaxRate = itemReq.TaxRate,
                    CompanyId = companyId.Value
                };
                item.Calculate();
                invoice.Items.Add(item);
            }

            // Calculate totals with GST
            string supplierState = company.Address?.GetStateCode() ?? string.Empty;
            string customerState = customer.BillingAddress?.GetStateCode() ?? string.Empty;
            invoice.Recalculate(supplierState, customerState);

            _context.Invoices.Add(invoice);

            // Update company's next invoice number using normal EF update
            company.NextInvoiceNumber++;
            _context.Companies.Update(company);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(invoice.Id);
        }
        catch (Exception ex)
        {
            var innerEx = ex.InnerException?.Message ?? "No inner exception";
            return Result<InvoiceResponse>.Failure($"Failed to create invoice: {ex.Message} | Inner: {innerEx}");
        }
    }

    public async Task<Result<InvoiceResponse>> GetByIdAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return Result<InvoiceResponse>.Failure("Invoice not found.");
        }

        return Result<InvoiceResponse>.Success(MapToResponse(invoice));
    }

    public async Task<PagedResult<InvoiceListResponse>> GetAllAsync(
        int page = 1, int pageSize = 20, InvoiceStatus? status = null, Guid? customerId = null,
        DateTime? from = null, DateTime? to = null, string? sortBy = null, bool sortDesc = false)
    {
        var query = _context.Invoices
            .Where(i => i.CompanyId == _companyService.CompanyId)
            .Include(i => i.Customer)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= to.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "number" => sortDesc ? query.OrderByDescending(i => i.InvoiceNumber) : query.OrderBy(i => i.InvoiceNumber),
            "date" => sortDesc ? query.OrderByDescending(i => i.InvoiceDate) : query.OrderBy(i => i.InvoiceDate),
            "amount" => sortDesc ? query.OrderByDescending(i => i.TotalAmount) : query.OrderBy(i => i.TotalAmount),
            "duedate" => sortDesc ? query.OrderByDescending(i => i.DueDate) : query.OrderBy(i => i.DueDate),
            "customer" => sortDesc ? query.OrderByDescending(i => i.Customer.Name) : query.OrderBy(i => i.Customer.Name),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(i => new InvoiceListResponse(
            i.Id, i.InvoiceNumber, i.Type, i.Status,
            i.InvoiceDate, i.DueDate,
            i.CustomerId, i.Customer.Name,
            i.TotalAmount, i.BalanceDue, i.Currency,
            i.CreatedAt
        )).ToList().AsReadOnly();

        return new PagedResult<InvoiceListResponse>(responses, totalCount, page, pageSize);
    }

    public async Task<Result<InvoiceResponse>> UpdateAsync(Guid id, UpdateInvoiceRequest request)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return Result<InvoiceResponse>.Failure("Invoice not found.");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return Result<InvoiceResponse>.Failure("Only draft invoices can be edited.");
        }

        // Load company and customer for GST
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == invoice.CompanyId && !c.IsDeleted);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
        if (customer is null)
        {
            return Result<InvoiceResponse>.Failure("Customer not found.");
        }

        invoice.CustomerId = request.CustomerId;
        invoice.DueDate = request.DueDate;
        invoice.DiscountPercentage = request.DiscountPercentage;
        invoice.Notes = request.Notes;
        invoice.TermsAndConditions = request.TermsAndConditions;
        invoice.ReferenceNumber = request.ReferenceNumber;

        // Replace items
        _context.InvoiceItems.RemoveRange(invoice.Items);
        invoice.Items.Clear();

        foreach (var itemReq in request.Items)
        {
            var item = new InvoiceItem
            {
                ProductId = itemReq.ProductId,
                Description = itemReq.Description,
                HsnSacCode = itemReq.HsnSacCode,
                Quantity = itemReq.Quantity,
                Unit = itemReq.Unit,
                Rate = itemReq.Rate,
                DiscountPercentage = itemReq.DiscountPercentage,
                TaxRate = itemReq.TaxRate,
                CompanyId = invoice.CompanyId,
                InvoiceId = invoice.Id
            };
            item.Calculate();
            invoice.Items.Add(item);
        }

        // Recalculate totals
        string supplierState = company.Address?.GetStateCode() ?? string.Empty;
        string customerState = customer.BillingAddress?.GetStateCode() ?? string.Empty;
        invoice.Recalculate(supplierState, customerState);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(invoice.Id);
    }

    public async Task<Result<InvoiceResponse>> DuplicateAsync(Guid id)
    {
        var source = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (source is null)
        {
            return Result<InvoiceResponse>.Failure("Invoice not found.");
        }

        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == source.CompanyId && !c.IsDeleted);

        var customer = await _context.Customers.FirstAsync(c => c.Id == source.CustomerId);

        string invoiceNumber = company.GenerateInvoiceNumber();

        var duplicate = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            Type = source.Type,
            Status = InvoiceStatus.Draft,
            InvoiceDate = DateTime.UtcNow,
            DueDate = source.DueDate.HasValue
                ? DateTime.UtcNow.AddDays((source.DueDate.Value - source.InvoiceDate).Days)
                : null,
            CustomerId = source.CustomerId,
            DiscountPercentage = source.DiscountPercentage,
            Notes = source.Notes,
            TermsAndConditions = source.TermsAndConditions,
            Currency = source.Currency
        };

        foreach (var srcItem in source.Items)
        {
            var item = new InvoiceItem
            {
                ProductId = srcItem.ProductId,
                Description = srcItem.Description,
                HsnSacCode = srcItem.HsnSacCode,
                Quantity = srcItem.Quantity,
                Unit = srcItem.Unit,
                Rate = srcItem.Rate,
                DiscountPercentage = srcItem.DiscountPercentage,
                TaxRate = srcItem.TaxRate,
                CompanyId = source.CompanyId
            };
            item.Calculate();
            duplicate.Items.Add(item);
        }

        string supplierState = company.Address?.GetStateCode() ?? string.Empty;
        string customerState = customer.BillingAddress?.GetStateCode() ?? string.Empty;
        duplicate.Recalculate(supplierState, customerState);

        _context.Invoices.Add(duplicate);
        await _context.Database.ExecuteSqlRawAsync(
            "UPDATE \"Companies\" SET \"NextInvoiceNumber\" = {0} WHERE \"Id\" = {1}",
            company.NextInvoiceNumber, company.Id);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(duplicate.Id);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return Result<bool>.Failure("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<bool>.Failure("Cannot delete a paid invoice.");
        }

        invoice.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static InvoiceResponse MapToResponse(Invoice inv)
    {
        var items = inv.Items.Select(i => new InvoiceItemResponse(
            i.Id, i.ProductId, i.Product?.Name,
            i.Description, i.HsnSacCode,
            i.Quantity, i.Unit, i.Rate,
            i.DiscountPercentage, i.DiscountAmount,
            i.TaxRate, i.TaxAmount, i.Amount
        )).ToList();

        return new InvoiceResponse(
            Id: inv.Id,
            InvoiceNumber: inv.InvoiceNumber,
            Type: inv.Type,
            Status: inv.Status,
            InvoiceDate: inv.InvoiceDate,
            DueDate: inv.DueDate,
            CustomerId: inv.CustomerId,
            CustomerName: inv.Customer.Name,
            SubTotal: inv.SubTotal,
            DiscountPercentage: inv.DiscountPercentage,
            DiscountAmount: inv.DiscountAmount,
            TaxAmount: inv.TaxAmount,
            CgstAmount: inv.CgstAmount,
            SgstAmount: inv.SgstAmount,
            IgstAmount: inv.IgstAmount,
            TotalAmount: inv.TotalAmount,
            AmountPaid: inv.AmountPaid,
            BalanceDue: inv.BalanceDue,
            Currency: inv.Currency,
            GstType: inv.GstType,
            Notes: inv.Notes,
            TermsAndConditions: inv.TermsAndConditions,
            ReferenceNumber: inv.ReferenceNumber,
            Items: items,
            CreatedAt: inv.CreatedAt
        );
    }
}
