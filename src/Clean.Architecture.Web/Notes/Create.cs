using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Create;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Create(IMediator mediator) : Endpoint<CreateNoteRequest, Results<Created<NoteRecord>, ValidationProblem, ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post(CreateNoteRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Create a new note";
      s.Description = "Creates a new note with the provided title and content.";
      s.ExampleRequest = new CreateNoteRequest { Title = "Test Note", Content = "This is a test note." };
      s.ResponseExamples[201] = new NoteRecord(Guid.NewGuid(), "Test Note", "This is a test note.", DateTime.UtcNow);

      // Document possible responses
      s.Responses[201] = "Note created successfully";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Accepts<CreateNoteRequest>("application/json")
      .Produces<NoteRecord>(201, "application/json")
      .ProducesProblem(500));
  }

  public override async Task<Results<Created<NoteRecord>, ValidationProblem, ProblemHttpResult>> ExecuteAsync(CreateNoteRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CreateNoteCommand(request.Title, request.Content), cancellationToken);

    return result.ToCreatedResult(
      id => $"/api/notes/{id.Value}",
      id => new NoteRecord(id.Value, request.Title, request.Content, DateTime.UtcNow));
  }
}

public class CreateNoteRequest
{
  public const string Route = "/api/notes";

  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
}