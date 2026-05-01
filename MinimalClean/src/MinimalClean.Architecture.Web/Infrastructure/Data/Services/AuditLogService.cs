using Ardalis.Result;
using Ardalis.Specification;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate.Specifications;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.Infrastructure.Data.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IRepository<AuditLog> _repository;
    private readonly IReadRepository<AuditLog> _readRepository;

    public AuditLogService(
        IRepository<AuditLog> repository,
        IReadRepository<AuditLog> readRepository)
    {
        _repository = repository;
        _readRepository = readRepository;
    }

    public async Task<Result<AuditLog>> CreateAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = AuditLog.Create(
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

            await _repository.AddAsync(auditLog, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result.Success(auditLog);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<AuditLog?>> GetByIdAsync(AuditLogId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new AuditLogByIdSpec(id);
            var auditLog = await _readRepository.FirstOrDefaultAsync(spec, cancellationToken);
            return Result.Success(auditLog);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<PagedResult<AuditLog>>> GetListAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? userId = null,
        string? endpoint = null,
        bool? isSuccess = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var spec = new AuditLogPagedFilterSpec(
                pageNumber,
                pageSize,
                userId,
                endpoint,
                isSuccess,
                startTime,
                endTime);

            var items = await _readRepository.ListAsync(spec, cancellationToken);

            var countSpec = new AuditLogCountFilterSpec(
                userId,
                endpoint,
                isSuccess,
                startTime,
                endTime);

            var totalCount = await _readRepository.CountAsync(countSpec, cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var result = new PagedResult<AuditLog>(
                items,
                pageNumber,
                pageSize,
                totalCount,
                totalPages);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<int>> CleanupAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        try
        {
            if (retentionDays < 1)
            {
                return Result.Error("Retention days must be at least 1");
            }

            var cutoffTime = DateTimeOffset.UtcNow.AddDays(-retentionDays);
            var spec = new AuditLogCleanupSpec(cutoffTime);

            var logsToDelete = await _readRepository.ListAsync(spec, cancellationToken);
            var count = logsToDelete.Count;

            foreach (var log in logsToDelete)
            {
                await _repository.DeleteAsync(log, cancellationToken);
            }

            await _repository.SaveChangesAsync(cancellationToken);

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}

public class AuditLogCountFilterSpec : Specification<AuditLog>
{
    public AuditLogCountFilterSpec(
        string? userId = null,
        string? endpoint = null,
        bool? isSuccess = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            Query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            Query.Where(a => a.Endpoint.Contains(endpoint));
        }

        if (isSuccess.HasValue)
        {
            Query.Where(a => a.IsSuccess == isSuccess.Value);
        }

        if (startTime.HasValue)
        {
            Query.Where(a => a.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            Query.Where(a => a.CreatedAt <= endTime.Value);
        }
    }
}
