using Microsoft.EntityFrameworkCore;
using SmartInvoice.Application.Customers;
using SmartInvoice.Application.Customers.DTOs;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public class CustomerLedgerService : ICustomerLedgerService
{
    private readonly AppDbContext _context;

    public CustomerLedgerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddEntryAsync(Guid customerId, LedgerEntryType type, Guid? referenceId, string? referenceNumber, decimal debit, decimal credit, string? notes = null)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null) return;

        // Get current running balance (last entry or opening balance)
        var lastEntry = await _context.CustomerLedgerEntries
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        decimal previousBalance = lastEntry?.RunningBalance ?? customer.OpeningBalance;
        decimal newBalance = previousBalance + debit - credit;

        var entry = new CustomerLedgerEntry
        {
            CustomerId = customerId,
            Date = DateTime.UtcNow,
            Type = type,
            ReferenceId = referenceId,
            ReferenceNumber = referenceNumber,
            Debit = debit,
            Credit = credit,
            RunningBalance = newBalance,
            Notes = notes
        };

        _context.CustomerLedgerEntries.Add(entry);

        // Update customer outstanding balance
        customer.OutstandingBalance = newBalance;

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CustomerLedgerEntryResponse>> GetLedgerAsync(Guid customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.CustomerLedgerEntries
            .Where(e => e.CustomerId == customerId);

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value.Date.AddDays(1));

        var entries = await query
            .OrderBy(e => e.Date)
            .ThenBy(e => e.CreatedAt)
            .ToListAsync();

        return entries.Select(e => new CustomerLedgerEntryResponse(
            e.Id, e.Date, e.Type, e.ReferenceId, e.ReferenceNumber,
            e.Debit, e.Credit, e.RunningBalance, e.Notes
        )).ToList().AsReadOnly();
    }

    public async Task RecalculateBalanceAsync(Guid customerId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null) return;

        var entries = await _context.CustomerLedgerEntries
            .Where(e => e.CustomerId == customerId)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.CreatedAt)
            .ToListAsync();

        decimal balance = customer.OpeningBalance;

        foreach (var entry in entries)
        {
            balance += entry.Debit - entry.Credit;
            entry.RunningBalance = balance;
        }

        customer.OutstandingBalance = balance;
        await _context.SaveChangesAsync();
    }
}
