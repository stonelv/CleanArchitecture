using FastEndpoints;
using FluentValidation;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.AuditLogFeatures.List;

public sealed class ListAuditLogsRequest
{
    [BindFrom("page")]
    public int Page { get; init; } = 1;

    [BindFrom("per_page")]
    public int PerPage { get; init; } = Constants.DEFAULT_PAGE_SIZE;

    [BindFrom("user_id")]
    public string? UserId { get; init; }

    [BindFrom("endpoint")]
    public string? Endpoint { get; init; }

    [BindFrom("is_success")]
    public bool? IsSuccess { get; init; }

    [BindFrom("start_time")]
    public DateTimeOffset? StartTime { get; init; }

    [BindFrom("end_time")]
    public DateTimeOffset? EndTime { get; init; }
}

public class ListEndpoint(IAuditLogService auditLogService)
    : Endpoint<ListAuditLogsRequest, AuditLogListResponse, ListAuditLogsMapper>
{
    private readonly IAuditLogService _auditLogService = auditLogService;

    public override void Configure()
    {
        Get("/AuditLogs");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "List audit logs with pagination and filtering";
            s.Description = "Retrieves a paginated list of audit logs. Supports filtering by user ID, endpoint, success status, and date range.";
            s.ExampleRequest = new ListAuditLogsRequest { Page = 1, PerPage = 10 };

            s.Params["page"] = "1-based page index (default 1)";
            s.Params["per_page"] = $"Page size 1–{Constants.MAX_PAGE_SIZE} (default {Constants.DEFAULT_PAGE_SIZE})";
            s.Params["user_id"] = "Filter by user ID";
            s.Params["endpoint"] = "Filter by endpoint name (partial match)";
            s.Params["is_success"] = "Filter by success status (true/false)";
            s.Params["start_time"] = "Filter by start time (ISO 8601 format)";
            s.Params["end_time"] = "Filter by end time (ISO 8601 format)";

            s.Responses[200] = "Paginated list of audit logs returned successfully";
            s.Responses[400] = "Invalid pagination parameters";
        });

        Tags("AuditLogs");

        Description(builder => builder
            .Accepts<ListAuditLogsRequest>()
            .Produces<AuditLogListResponse>(200, "application/json")
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(ListAuditLogsRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetListAsync(
            pageNumber: request.Page,
            pageSize: request.PerPage,
            userId: request.UserId,
            endpoint: request.Endpoint,
            isSuccess: request.IsSuccess,
            startTime: request.StartTime,
            endTime: request.EndTime,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess)
        {
            await Send.ErrorsAsync(statusCode: 400, cancellationToken);
            return;
        }

        var pagedResult = result.Value;
        AddLinkHeader(pagedResult.Page, pagedResult.PerPage, pagedResult.TotalPages);

        var response = Map.FromEntity(pagedResult);
        await Send.OkAsync(response, cancellationToken);
    }

    private void AddLinkHeader(int page, int perPage, int totalPages)
    {
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
        var queryString = HttpContext.Request.QueryString.ToString();
        var existingParams = System.Web.HttpUtility.ParseQueryString(queryString);
        existingParams.Remove("page");
        existingParams.Remove("per_page");
        var baseQuery = existingParams.Count > 0 ? "?" + existingParams.ToString() : "";

        string Link(string rel, int p)
        {
            var sep = string.IsNullOrEmpty(baseQuery) ? "?" : "&";
            return $"<{baseUrl}{baseQuery}{sep}page={p}&per_page={perPage}>; rel=\"{rel}\"";
        }

        var parts = new List<string>();
        if (page > 1)
        {
            parts.Add(Link("first", 1));
            parts.Add(Link("prev", page - 1));
        }
        if (page < totalPages)
        {
            parts.Add(Link("next", page + 1));
            parts.Add(Link("last", totalPages));
        }

        if (parts.Count > 0)
            HttpContext.Response.Headers["Link"] = string.Join(", ", parts);
    }
}

public sealed class ListAuditLogsValidator : Validator<ListAuditLogsRequest>
{
    public ListAuditLogsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("page must be >= 1");

        RuleFor(x => x.PerPage)
            .InclusiveBetween(1, Constants.MAX_PAGE_SIZE)
            .WithMessage($"per_page must be between 1 and {Constants.MAX_PAGE_SIZE}");
    }
}

public sealed class ListAuditLogsMapper
    : Mapper<ListAuditLogsRequest, AuditLogListResponse, PagedResult<AuditLog>>
{
    public override AuditLogListResponse FromEntity(PagedResult<AuditLog> e)
    {
        var items = e.Items
            .Select(ToAuditLogRecord)
            .ToList();

        return new AuditLogListResponse(items, e.Page, e.PerPage, e.TotalCount, e.TotalPages);
    }

    private static AuditLogRecord ToAuditLogRecord(AuditLog auditLog)
    {
        return new AuditLogRecord(
            Id: auditLog.Id.Value,
            UserId: auditLog.UserId,
            Endpoint: auditLog.Endpoint,
            HttpMethod: auditLog.HttpMethod,
            ElapsedMilliseconds: auditLog.ElapsedMilliseconds,
            StatusCode: auditLog.StatusCode,
            IsSuccess: auditLog.IsSuccess,
            ExceptionMessage: auditLog.ExceptionMessage,
            ClientIp: auditLog.ClientIp,
            UserAgent: auditLog.UserAgent,
            RequestPath: auditLog.RequestPath,
            RequestMethod: auditLog.RequestMethod,
            CreatedAt: auditLog.CreatedAt);
    }
}
