using System.Diagnostics;
using System.Net;
using System.Text.Json;
using SmartInvoice.Infrastructure.Services;

namespace SmartInvoice.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLogService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            // Get caller information
            var stackFrame = new StackTrace(ex, true).GetFrame(0);
            var sourceFile = stackFrame?.GetFileName() ?? "Unknown";
            var sourceMethod = stackFrame?.GetMethod()?.Name ?? "Unknown";
            var lineNumber = stackFrame?.GetFileLineNumber() ?? 0;

            // Extract request payload if available
            var requestPayload = string.Empty;
            if (context.Request.ContentLength > 0)
            {
                context.Request.Body.Position = 0;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    requestPayload = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
            }

            // Log to database asynchronously
            var parsedCompanyId = Guid.TryParse(context.User?.FindFirst("CompanyId")?.Value, out var cId) ? cId : (Guid?)null;
            _ = errorLogService.LogErrorAsync(
                ex,
                sourceMethod,
                sourceFile,
                lineNumber,
                userId: context.User?.FindFirst("sub")?.Value,
                companyId: parsedCompanyId,
                requestUrl: context.Request.Path,
                requestMethod: context.Request.Method,
                httpStatusCode: context.Response.StatusCode,
                requestPayload: requestPayload);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
