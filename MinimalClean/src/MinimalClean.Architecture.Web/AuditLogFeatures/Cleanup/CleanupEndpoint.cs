using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.AuditLogFeatures.Cleanup;

public sealed class CleanupAuditLogsRequest
{
    public const string Route = "/AuditLogs/Cleanup";

    [BindFrom("retention_days")]
    public int RetentionDays { get; init; } = 30;
}

public class CleanupEndpoint(IAuditLogService auditLogService)
    : Endpoint<CleanupAuditLogsRequest,
               Results<Ok<CleanupResponse>,
                       ProblemHttpResult>>
{
    private readonly IAuditLogService _auditLogService = auditLogService;

    public override void Configure()
    {
        Post(CleanupAuditLogsRequest.Route);
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Cleanup old audit logs";
            s.Description = "Deletes audit logs that are older than the specified retention period (in days). Returns the number of deleted logs.";
            s.ExampleRequest = new CleanupAuditLogsRequest { RetentionDays = 30 };

            s.Params["retention_days"] = "Number of days to retain logs (default 30, minimum 1)";

            s.Responses[200] = "Cleanup completed successfully";
            s.Responses[400] = "Invalid retention days parameter";
        });

        Tags("AuditLogs");

        Description(builder => builder
            .Accepts<CleanupAuditLogsRequest>()
            .Produces<CleanupResponse>(200, "application/json")
            .ProducesProblem(400));
    }

    public override async Task<Results<Ok<CleanupResponse>, ProblemHttpResult>>
        ExecuteAsync(CleanupAuditLogsRequest request, CancellationToken ct)
    {
        var result = await _auditLogService.CleanupAsync(request.RetentionDays, ct);

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(result.Errors.FirstOrDefault() ?? "An error occurred during cleanup");
        }

        var response = new CleanupResponse(result.Value);
        return TypedResults.Ok(response);
    }
}

public sealed class CleanupAuditLogsValidator : Validator<CleanupAuditLogsRequest>
{
    public CleanupAuditLogsValidator()
    {
        RuleFor(x => x.RetentionDays)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Retention days must be at least 1");
    }
}
