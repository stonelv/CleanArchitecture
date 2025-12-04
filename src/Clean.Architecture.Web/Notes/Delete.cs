using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Delete;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Delete(IMediator mediator) : Endpoint<DeleteNoteRequest, Results<NoContent, NotFound, ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Delete(DeleteNoteRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Delete a note";
      s.Description = "Deletes a specific note by its ID.";

      // Document possible responses
      s.Responses[204] = "Note deleted successfully";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Produces(204)
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<NoContent, NotFound, ProblemHttpResult>> ExecuteAsync(DeleteNoteRequest request, CancellationToken cancellationToken)
  {
    var noteId = NoteId.From(request.Id);
    var result = await _mediator.Send(new DeleteNoteCommand(noteId), cancellationToken);

    return result.ToDeleteResult();
  }
}

public class DeleteNoteRequest
{
  public const string Route = "/api/notes/{Id:guid}";

  public Guid Id { get; set; }
}
