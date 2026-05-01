using System.Diagnostics;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.Configurations;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTimeOffset.UtcNow;
        Exception? exception = null;
        int statusCode = StatusCodes.Status200OK;

        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            exception = ex;
            statusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                var endpoint = context.GetEndpoint();
                var endpointName = endpoint?.DisplayName ?? context.Request.Path;
                
                var userId = GetUserId(context);
                var clientIp = GetClientIpAddress(context);
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                
                var isSuccess = statusCode >= 200 && statusCode < 400;

                await auditLogService.CreateAsync(
                    userId: userId,
                    endpoint: endpointName,
                    httpMethod: context.Request.Method,
                    elapsedMilliseconds: stopwatch.ElapsedMilliseconds,
                    statusCode: statusCode,
                    isSuccess: isSuccess,
                    exceptionMessage: exception?.Message,
                    exceptionStackTrace: exception?.StackTrace,
                    clientIp: clientIp,
                    userAgent: userAgent,
                    requestPath: context.Request.Path,
                    requestMethod: context.Request.Method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log");
            }
        }
    }

    private static string? GetUserId(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }
        return null;
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        
        if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
        {
            ipAddress = context.Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault();
        }
        else if (!string.IsNullOrEmpty(context.Request.Headers["X-Real-IP"]))
        {
            ipAddress = context.Request.Headers["X-Real-IP"].ToString();
        }

        return ipAddress;
    }
}
