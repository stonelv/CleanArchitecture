using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Get;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class GetById(IMediator mediator) : Endpoint<GetNoteByIdRequest, Results<Ok<NoteRecord>, NotFound, ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get(GetNoteByIdRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get a note by ID";
      s.Description = "Retrieves a specific note by its ID.";
      s.ResponseExamples[200] = new NoteRecord(Guid.NewGuid(), "Test Note", "This is a test note.", DateTime.UtcNow);

      // Document possible responses
      s.Responses[200] = "Note retrieved successfully";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Produces<NoteRecord>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<Ok<NoteRecord>, NotFound, ProblemHttpResult>> ExecuteAsync(GetNoteByIdRequest request, CancellationToken cancellationToken)
  {
    var noteId = NoteId.From(request.Id);
    var result = await _mediator.Send(new GetNoteQuery(noteId), cancellationToken);

    return result.ToGetByIdResult(noteDto => new NoteRecord(noteDto.Id.Value, noteDto.Title, noteDto.Content, noteDto.CreatedOn));
  }
}

public class GetNoteByIdRequest
{
  public const string Route = "/api/notes/{Id:guid}";

  public Guid Id { get; set; }
}