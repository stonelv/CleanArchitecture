using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.List;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class List(IMediator mediator) : Endpoint<GetNotesRequest, Ok<IEnumerable<NoteRecord>>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get(GetNotesRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get all notes";
      s.Description = "Retrieves a list of all notes.";
      s.ResponseExamples[200] = new[]
      {
        new NoteRecord(Guid.NewGuid(), "Test Note 1", "This is the first test note.", DateTime.UtcNow),
        new NoteRecord(Guid.NewGuid(), "Test Note 2", "This is the second test note.", DateTime.UtcNow)
      };

      // Document possible responses
      s.Responses[200] = "Notes retrieved successfully";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Produces<IEnumerable<NoteRecord>>(200, "application/json")
      .ProducesProblem(500));
  }

  public override async Task<Ok<IEnumerable<NoteRecord>>> ExecuteAsync(GetNotesRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetNotesQuery(), cancellationToken);

    if (result.Status != ResultStatus.Ok)
    {
      throw new Exception(string.Join(";", result.Errors));
    }

    return result.ToOkOnlyResult(noteDtos => noteDtos.Select(noteDto => new NoteRecord(noteDto.Id.Value, noteDto.Title, noteDto.Content, noteDto.CreatedOn)));
  }
}

public class GetNotesRequest
{
  public const string Route = "/api/notes";
}
