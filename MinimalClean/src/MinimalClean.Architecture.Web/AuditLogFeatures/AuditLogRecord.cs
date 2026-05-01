namespace MinimalClean.Architecture.Web.AuditLogFeatures;

public record AuditLogRecord(
    Guid Id,
    string? UserId,
    string Endpoint,
    string HttpMethod,
    long ElapsedMilliseconds,
    int? StatusCode,
    bool IsSuccess,
    string? ExceptionMessage,
    string? ClientIp,
    string? UserAgent,
    string? RequestPath,
    string? RequestMethod,
    DateTimeOffset CreatedAt);

public record AuditLogListResponse : PagedResult<AuditLogRecord>
{
    public AuditLogListResponse(
        IReadOnlyList<AuditLogRecord> Items,
        int Page,
        int PerPage,
        int TotalCount,
        int TotalPages)
        : base(Items, Page, PerPage, TotalCount, TotalPages)
    {
    }
}

public record CleanupResponse(int DeletedCount);
