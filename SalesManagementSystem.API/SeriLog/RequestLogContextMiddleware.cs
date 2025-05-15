using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace SalesManagementSystem.API.SeriLog;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {

        var start = Stopwatch.GetTimestamp();
        Exception exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            LogRequest(context, elapsed, exception, context.User.FindFirstValue("Id"));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogRequest(HttpContext context, TimeSpan elapsed, Exception exception, string userId = null)
    {
        var statusCode = context.Response.StatusCode;
        var logLevel = exception != null ? LogLevel.Error :
            statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        if (!_logger.IsEnabled(logLevel)) return;

        _logger.Log(logLevel, exception,
            "HTTP {Method} {Path} {StatusCode} in {Elapsed}ms UserId {UserId}",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsed.TotalMilliseconds,
            userId);
    }


}