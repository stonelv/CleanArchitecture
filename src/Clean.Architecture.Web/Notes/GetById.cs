using System.ComponentModel.DataAnnotations;
using Clean.Architecture.UseCases.Notes.Get;
using Clean.Architecture.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class GetById(IMediator mediator)
  : Endpoint<GetNoteByIdRequest, 
          Results<Ok<GetNoteByIdResponse>, 
                  NotFound, 
                  ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get(GetNoteByIdRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get a note by id";
      s.Description = "Gets a note with the provided id.";
      s.ResponseExamples[200] = new GetNoteByIdResponse(Guid.NewGuid(), "Test Note", "This is a test note.", DateTime.Now);

      // Document possible responses
      s.Responses[200] = "Note found successfully";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Produces<GetNoteByIdResponse>(200, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<Ok<GetNoteByIdResponse>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetNoteByIdRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetNoteByIdQuery(request.Id));

    return result.ToGetByIdResult(
      noteDto => new GetNoteByIdResponse(noteDto.Id, noteDto.Title, noteDto.Content, noteDto.CreatedOn));
  }
}

public class GetNoteByIdRequest
{
  public const string Route = "/Notes/{Id}";

  [Required]
  public Guid Id { get; set; }
}

public class GetNoteByIdValidator : Validator<GetNoteByIdRequest>
{
  public GetNoteByIdValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("Id is required.");
  }
}

public class GetNoteByIdResponse(Guid id, string title, string content, DateTime createdOn)
{
  public Guid Id { get; set; } = id;
  public string Title { get; set; } = title;
  public string Content { get; set; } = content;
  public DateTime CreatedOn { get; set; } = createdOn;
}
