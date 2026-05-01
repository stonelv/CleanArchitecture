namespace MinimalClean.Architecture.Web.Domain.AuditLogAggregate.Specifications;

public class AuditLogCleanupSpec : Specification<AuditLog>
{
    public AuditLogCleanupSpec(DateTimeOffset cutoffTime)
    {
        Query.Where(a => a.CreatedAt < cutoffTime);
    }
}
