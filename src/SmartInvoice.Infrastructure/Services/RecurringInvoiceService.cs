using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Common;
using SmartInvoice.Application.Invoices;
using SmartInvoice.Application.Invoices.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class RecurringInvoiceService : IRecurringInvoiceService
{
    private readonly AppDbContext _context;
    private readonly IInvoiceService _invoiceService;

    public RecurringInvoiceService(AppDbContext context, IInvoiceService invoiceService)
    {
        _context = context;
        _invoiceService = invoiceService;
    }

    public async Task<Result<RecurringInvoiceResponse>> CreateAsync(CreateRecurringInvoiceRequest request)
    {
        var nextDate = CalculateNextDate(DateTime.UtcNow, request.Frequency);

        var recurring = new RecurringInvoice
        {
            CustomerId = request.CustomerId,
            Frequency = request.Frequency,
            InvoiceType = request.InvoiceType,
            NextGenerationDate = nextDate,
            IsActive = true,
            Notes = request.Notes,
            TermsAndConditions = request.TermsAndConditions,
            DiscountPercentage = request.DiscountPercentage,
            ItemsJson = JsonSerializer.Serialize(request.Items)
        };

        _context.Set<RecurringInvoice>().Add(recurring);
        await _context.SaveChangesAsync();

        var customer = await _context.Customers.FirstAsync(c => c.Id == request.CustomerId);

        return Result<RecurringInvoiceResponse>.Success(new RecurringInvoiceResponse(
            recurring.Id, recurring.CustomerId, customer.Name,
            recurring.Frequency, recurring.InvoiceType,
            recurring.NextGenerationDate, recurring.LastGeneratedDate,
            recurring.IsActive, recurring.Notes, recurring.DiscountPercentage,
            request.Items.Count
        ));
    }

    public async Task<IReadOnlyList<RecurringInvoiceResponse>> GetAllAsync()
    {
        var items = await _context.Set<RecurringInvoice>()
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return items.Select(r =>
        {
            var itemCount = 0;
            try { itemCount = JsonSerializer.Deserialize<List<object>>(r.ItemsJson)?.Count ?? 0; } catch { }
            return new RecurringInvoiceResponse(
                r.Id, r.CustomerId, r.Customer.Name,
                r.Frequency, r.InvoiceType,
                r.NextGenerationDate, r.LastGeneratedDate,
                r.IsActive, r.Notes, r.DiscountPercentage, itemCount
            );
        }).ToList().AsReadOnly();
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id)
    {
        var recurring = await _context.Set<RecurringInvoice>().FirstOrDefaultAsync(r => r.Id == id);
        if (recurring is null) return Result<bool>.Failure("Recurring invoice not found.");
        recurring.IsActive = false;
        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Called by Hangfire daily job. Finds all active recurring invoices due today or earlier, generates invoices.
    /// </summary>
    public async Task<int> ProcessDueRecurringInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var dueItems = await _context.Set<RecurringInvoice>()
            .Where(r => r.IsActive && r.NextGenerationDate <= today)
            .ToListAsync();

        int generated = 0;

        foreach (var recurring in dueItems)
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<InvoiceItemRequest>>(recurring.ItemsJson) ?? [];

                var request = new CreateInvoiceRequest(
                    CustomerId: recurring.CustomerId,
                    Type: recurring.InvoiceType,
                    DueDate: null,
                    DiscountPercentage: recurring.DiscountPercentage,
                    Notes: recurring.Notes,
                    TermsAndConditions: recurring.TermsAndConditions,
                    ReferenceNumber: $"RECURRING-{recurring.Id.ToString()[..8]}",
                    Items: items
                );

                await _invoiceService.CreateAsync(request);

                recurring.LastGeneratedDate = DateTime.UtcNow;
                recurring.NextGenerationDate = CalculateNextDate(DateTime.UtcNow, recurring.Frequency);
                generated++;
            }
            catch
            {
                // Log and continue with next recurring invoice
            }
        }

        await _context.SaveChangesAsync();
        return generated;
    }

    private static DateTime CalculateNextDate(DateTime from, RecurrenceFrequency freq) => freq switch
    {
        RecurrenceFrequency.Daily => from.AddDays(1),
        RecurrenceFrequency.Weekly => from.AddDays(7),
        RecurrenceFrequency.Monthly => from.AddMonths(1),
        RecurrenceFrequency.Quarterly => from.AddMonths(3),
        RecurrenceFrequency.Yearly => from.AddYears(1),
        _ => from.AddMonths(1)
    };
}
