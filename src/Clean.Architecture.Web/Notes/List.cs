using Clean.Architecture.UseCases.Notes.List;
using Clean.Architecture.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class List(IMediator mediator)
  : Endpoint<ListNotesRequest, 
          Ok<GetNotesListResponse>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get(ListNotesRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get a list of notes";
      s.Description = "Gets a list of notes with optional pagination.";
      s.ResponseExamples[200] = new GetNotesListResponse(
        new List<GetNoteByIdResponse>
        {
          new GetNoteByIdResponse(Guid.NewGuid(), "Test Note 1", "This is a test note.", DateTime.Now),
          new GetNoteByIdResponse(Guid.NewGuid(), "Test Note 2", "This is another test note.", DateTime.Now.AddMinutes(1))
        },
        2,
        1,
        10);

      // Document possible responses
      s.Responses[200] = "Notes retrieved successfully";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Produces<GetNotesListResponse>(200, "application/json")
      .ProducesProblem(500));
  }

  public override async Task<Ok<GetNotesListResponse>>
    ExecuteAsync(ListNotesRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ListNotesQuery(request.Page, request.PerPage));

    return result.ToOkOnlyResult(
      pagedResult => new GetNotesListResponse(
        pagedResult.Items.Select(n => new GetNoteByIdResponse(n.Id, n.Title, n.Content, n.CreatedOn)).ToList(),
        pagedResult.TotalCount,
        pagedResult.Page,
        pagedResult.PerPage));
  }
}

public class ListNotesRequest
{
  public const string Route = "/Notes";

  public int? Page { get; set; } = 1;
  public int? PerPage { get; set; } = 10;
}

public class GetNotesListResponse
{
  public List<GetNoteByIdResponse> Items { get; set; } = new List<GetNoteByIdResponse>();
  public int TotalCount { get; set; } = 0;
  public int CurrentPage { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

  public GetNotesListResponse()
  {
  }

  public GetNotesListResponse(List<GetNoteByIdResponse> items, int totalCount, int currentPage, int pageSize)
  {
    Items = items;
    TotalCount = totalCount;
    CurrentPage = currentPage;
    PageSize = pageSize;
  }
}
