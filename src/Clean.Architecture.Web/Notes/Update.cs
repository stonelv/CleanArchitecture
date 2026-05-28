using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Update;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Update(IMediator mediator) : Endpoint<UpdateNoteRequest, Results<Ok<NoteRecord>, NotFound, ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Put(UpdateNoteRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Update a note";
      s.Description = "Updates an existing note with the provided title and content.";
      s.ExampleRequest = new UpdateNoteRequest { Id = Guid.NewGuid(), Title = "Updated Test Note", Content = "This is an updated test note." };
      s.ResponseExamples[200] = new NoteRecord(Guid.NewGuid(), "Updated Test Note", "This is an updated test note.", DateTime.UtcNow);

      // Document possible responses
      s.Responses[200] = "Note updated successfully";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Accepts<UpdateNoteRequest>("application/json")
      .Produces<NoteRecord>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<Ok<NoteRecord>, NotFound, ProblemHttpResult>> ExecuteAsync(UpdateNoteRequest request, CancellationToken cancellationToken)
  {
    var noteId = NoteId.From(request.Id);
    var result = await _mediator.Send(new UpdateNoteCommand(noteId, request.Title, request.Content), cancellationToken);

    return result.ToUpdateResult(noteDto => new NoteRecord(noteDto.Id.Value, noteDto.Title, noteDto.Content, noteDto.CreatedOn));
  }
}

public class UpdateNoteRequest
{
  public const string Route = "/api/notes/{Id:guid}";

  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
}
