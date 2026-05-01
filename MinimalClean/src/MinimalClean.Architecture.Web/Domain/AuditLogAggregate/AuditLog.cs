using Ardalis.GuardClauses;

namespace MinimalClean.Architecture.Web.Domain.AuditLogAggregate;

public class AuditLog : EntityBase<AuditLog, AuditLogId>, IAggregateRoot
{
    private AuditLog() { }

    private AuditLog(
        string? userId,
        string endpoint,
        string httpMethod,
        long elapsedMilliseconds,
        int? statusCode,
        bool isSuccess,
        string? exceptionMessage,
        string? exceptionStackTrace,
        string? clientIp,
        string? userAgent,
        string? requestPath,
        string? requestMethod)
    {
        UserId = userId;
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        ElapsedMilliseconds = elapsedMilliseconds;
        StatusCode = statusCode;
        IsSuccess = isSuccess;
        ExceptionMessage = exceptionMessage;
        ExceptionStackTrace = exceptionStackTrace;
        ClientIp = clientIp;
        UserAgent = userAgent;
        RequestPath = requestPath;
        RequestMethod = requestMethod;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public AuditLog(
        AuditLogId id,
        string? userId,
        string endpoint,
        string httpMethod,
        long elapsedMilliseconds,
        int? statusCode,
        bool isSuccess,
        string? exceptionMessage,
        string? exceptionStackTrace,
        string? clientIp,
        string? userAgent,
        string? requestPath,
        string? requestMethod,
        DateTimeOffset createdAt)
    {
        Guard.Against.InvalidInput(id, nameof(id), (id) => id != AuditLogId.From(Guid.Empty),
            "Use AuditLog.Create() to create new audit logs instead of passing empty Guid to the constructor.");
        
        Id = id;
        UserId = userId;
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        ElapsedMilliseconds = elapsedMilliseconds;
        StatusCode = statusCode;
        IsSuccess = isSuccess;
        ExceptionMessage = exceptionMessage;
        ExceptionStackTrace = exceptionStackTrace;
        ClientIp = clientIp;
        UserAgent = userAgent;
        RequestPath = requestPath;
        RequestMethod = requestMethod;
        CreatedAt = createdAt;
    }

    public static AuditLog Create(
        string? userId,
        string endpoint,
        string httpMethod,
        long elapsedMilliseconds,
        int? statusCode,
        bool isSuccess,
        string? exceptionMessage = null,
        string? exceptionStackTrace = null,
        string? clientIp = null,
        string? userAgent = null,
        string? requestPath = null,
        string? requestMethod = null)
    {
        Guard.Against.NullOrEmpty(endpoint, nameof(endpoint));
        Guard.Against.NullOrEmpty(httpMethod, nameof(httpMethod));
        Guard.Against.NegativeOrZero(elapsedMilliseconds, nameof(elapsedMilliseconds));

        return new AuditLog(
            userId,
            endpoint,
            httpMethod,
            elapsedMilliseconds,
            statusCode,
            isSuccess,
            exceptionMessage,
            exceptionStackTrace,
            clientIp,
            userAgent,
            requestPath,
            requestMethod);
    }

    public string? UserId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = string.Empty;
    public long ElapsedMilliseconds { get; private set; }
    public int? StatusCode { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ExceptionMessage { get; private set; }
    public string? ExceptionStackTrace { get; private set; }
    public string? ClientIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string? RequestPath { get; private set; }
    public string? RequestMethod { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
