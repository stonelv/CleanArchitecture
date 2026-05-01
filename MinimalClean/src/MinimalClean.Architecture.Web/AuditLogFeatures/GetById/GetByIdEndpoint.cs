using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate;
using MinimalClean.Architecture.Web.Domain.Interfaces;

namespace MinimalClean.Architecture.Web.AuditLogFeatures.GetById;

public sealed class GetAuditLogByIdRequest
{
    public const string Route = "/AuditLogs/{AuditLogId}";
    public Guid AuditLogId { get; init; }
}

public class GetByIdEndpoint(IAuditLogService auditLogService)
    : Endpoint<GetAuditLogByIdRequest,
               Results<Ok<AuditLogRecord>,
                       NotFound,
                       ProblemHttpResult>,
               GetAuditLogByIdMapper>
{
    private readonly IAuditLogService _auditLogService = auditLogService;

    public override void Configure()
    {
        Get(GetAuditLogByIdRequest.Route);
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get an audit log by ID";
            s.Description = "Retrieves a specific audit log entry by its unique identifier. Returns detailed information about the request.";
            s.ExampleRequest = new GetAuditLogByIdRequest { AuditLogId = Guid.NewGuid() };

            s.Responses[200] = "Audit log found and returned successfully";
            s.Responses[404] = "Audit log with specified ID not found";
        });

        Tags("AuditLogs");

        Description(builder => builder
            .Accepts<GetAuditLogByIdRequest>()
            .Produces<AuditLogRecord>(200, "application/json")
            .ProducesProblem(404));
    }

    public override async Task<Results<Ok<AuditLogRecord>, NotFound, ProblemHttpResult>>
        ExecuteAsync(GetAuditLogByIdRequest request, CancellationToken ct)
    {
        var result = await _auditLogService.GetByIdAsync(AuditLogId.From(request.AuditLogId), ct);

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(result.Errors.FirstOrDefault() ?? "An error occurred");
        }

        var auditLog = result.Value;
        if (auditLog == null)
        {
            return TypedResults.NotFound();
        }

        var response = Map.FromEntity(auditLog);
        return TypedResults.Ok(response);
    }
}

public sealed class GetAuditLogByIdValidator : Validator<GetAuditLogByIdRequest>
{
    public GetAuditLogByIdValidator()
    {
        RuleFor(x => x.AuditLogId)
            .NotEmpty()
            .WithMessage("Audit log ID cannot be empty");
    }
}

public sealed class GetAuditLogByIdMapper
    : Mapper<GetAuditLogByIdRequest, AuditLogRecord, AuditLog>
{
    public override AuditLogRecord FromEntity(AuditLog e)
        => new(
            Id: e.Id.Value,
            UserId: e.UserId,
            Endpoint: e.Endpoint,
            HttpMethod: e.HttpMethod,
            ElapsedMilliseconds: e.ElapsedMilliseconds,
            StatusCode: e.StatusCode,
            IsSuccess: e.IsSuccess,
            ExceptionMessage: e.ExceptionMessage,
            ClientIp: e.ClientIp,
            UserAgent: e.UserAgent,
            RequestPath: e.RequestPath,
            RequestMethod: e.RequestMethod,
            CreatedAt: e.CreatedAt);
}
