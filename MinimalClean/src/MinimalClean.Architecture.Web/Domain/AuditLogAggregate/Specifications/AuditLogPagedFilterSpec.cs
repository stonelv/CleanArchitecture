namespace MinimalClean.Architecture.Web.Domain.AuditLogAggregate.Specifications;

public class AuditLogPagedFilterSpec : Specification<AuditLog>
{
    public AuditLogPagedFilterSpec(
        int pageNumber,
        int pageSize,
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

        Query.OrderByDescending(a => a.CreatedAt);
        
        int skip = (pageNumber - 1) * pageSize;
        Query.Skip(skip).Take(pageSize);
    }
}
