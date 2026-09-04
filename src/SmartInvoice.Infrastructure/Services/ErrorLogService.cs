using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartInvoice.Domain.Entities;
using SmartInvoice.Infrastructure.Persistence;

namespace SmartInvoice.Infrastructure.Services;

public interface IErrorLogService
{
    Task LogErrorAsync(
        Exception exception,
        string sourceMethod,
        string sourceFile,
        int lineNumber,
        string? userId = null,
        Guid? companyId = null,
        string? requestUrl = null,
        string? requestMethod = null,
        int? httpStatusCode = null,
        string? requestPayload = null);
    
    Task<List<ErrorLog>> GetErrorsAsync(int pageSize = 50);
    Task<ErrorLog?> GetErrorAsync(Guid id);
    Task MarkAsResolvedAsync(Guid id, string resolution);
}

public class ErrorLogService : IErrorLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(AppDbContext context, ILogger<ErrorLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogErrorAsync(
        Exception exception,
        string sourceMethod,
        string sourceFile,
        int lineNumber,
        string? userId = null,
        Guid? companyId = null,
        string? requestUrl = null,
        string? requestMethod = null,
        int? httpStatusCode = null,
        string? requestPayload = null)
    {
        try
        {
            var errorLog = new ErrorLog
            {
                ErrorType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message,
                SourceMethod = sourceMethod,
                SourceFile = sourceFile,
                LineNumber = lineNumber,
                UserId = userId,
                CompanyId = companyId ?? Guid.Empty,
                RequestUrl = requestUrl,
                RequestMethod = requestMethod,
                HttpStatusCode = httpStatusCode,
                RequestPayload = requestPayload,
                LoggedAt = DateTime.UtcNow,
                IsResolved = false
            };

            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();

            _logger.LogError(
                $"Error logged: {exception.GetType().Name} - {exception.Message} | Method: {sourceMethod} | File: {sourceFile}:{lineNumber}");
        }
        catch (Exception ex)
        {
            // If logging itself fails, log to application logger only
            _logger.LogError(ex, "Failed to log error to database");
        }
    }

    public async Task<List<ErrorLog>> GetErrorsAsync(int pageSize = 50)
    {
        return await _context.ErrorLogs
            .IgnoreQueryFilters()
            .OrderByDescending(e => e.LoggedAt)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ErrorLog?> GetErrorAsync(Guid id)
    {
        return await _context.ErrorLogs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task MarkAsResolvedAsync(Guid id, string resolution)
    {
        var error = await GetErrorAsync(id);
        if (error is not null)
        {
            error.IsResolved = true;
            error.Resolution = resolution;
            error.ResolvedAt = DateTime.UtcNow;
            _context.ErrorLogs.Update(error);
            await _context.SaveChangesAsync();
        }
    }
}
