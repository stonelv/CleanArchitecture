using Ardalis.Result;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate;

namespace MinimalClean.Architecture.Web.Domain.Interfaces;

public interface IAuditLogService
{
    Task<Result<AuditLog>> CreateAsync(
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
        string? requestMethod = null,
        CancellationToken cancellationToken = default);

    Task<Result<AuditLog?>> GetByIdAsync(AuditLogId id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<AuditLog>>> GetListAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? userId = null,
        string? endpoint = null,
        bool? isSuccess = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        CancellationToken cancellationToken = default);

    Task<Result<int>> CleanupAsync(int retentionDays, CancellationToken cancellationToken = default);
}
