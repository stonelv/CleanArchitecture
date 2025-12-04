using System.ComponentModel.DataAnnotations;
using Clean.Architecture.UseCases.Notes.Delete;
using Clean.Architecture.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Delete(IMediator mediator)
  : Endpoint<DeleteNoteRequest, 
          Results<NoContent, 
                  NotFound, 
                  ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete(DeleteNoteRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Delete a note";
      s.Description = "Deletes a note with the provided id.";
      s.ExampleRequest = new DeleteNoteRequest { Id = Guid.NewGuid() };

      // Document possible responses
      s.Responses[204] = "Note deleted successfully (no content returned)";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Accepts<DeleteNoteRequest>("application/json")
      .Produces(204)
      .ProducesProblem(400)
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<NoContent, NotFound, ProblemHttpResult>>
    ExecuteAsync(DeleteNoteRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new DeleteNoteCommand(request.Id));

    return result.ToDeleteResult();
  }
}

public class DeleteNoteRequest
{
  public const string Route = "/Notes/{Id}";

  [Required]
  public Guid Id { get; set; }
}

public class DeleteNoteValidator : Validator<DeleteNoteRequest>
{
  public DeleteNoteValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("Id is required.");
  }
}
