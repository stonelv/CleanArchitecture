namespace MinimalClean.Architecture.Web.Domain.AuditLogAggregate.Specifications;

public class AuditLogByIdSpec : Specification<AuditLog>
{
    public AuditLogByIdSpec(AuditLogId auditLogId) =>
        Query.Where(a => a.Id == auditLogId);
}
