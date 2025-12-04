using System.ComponentModel.DataAnnotations;
using Clean.Architecture.UseCases.Notes.Create;
using Clean.Architecture.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Create(IMediator mediator)
  : Endpoint<CreateNoteRequest, 
          Results<Created<CreateNoteResponse>, 
                  ValidationProblem, 
                  ProblemHttpResult>>
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
      s.ResponseExamples[201] = new CreateNoteResponse(Guid.NewGuid(), "Test Note", "This is a test note.", DateTime.Now);

      // Document possible responses
      s.Responses[201] = "Note created successfully";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Accepts<CreateNoteRequest>("application/json")
      .Produces<CreateNoteResponse>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(500));
  }

  public override async Task<Results<Created<CreateNoteResponse>, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(CreateNoteRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CreateNoteCommand(request.Title!, request.Content!));

    return result.ToCreatedResult(
      id => $"/Notes/{id}",
      id => new CreateNoteResponse(id, request.Title!, request.Content!, DateTime.Now));
  }
}

public class CreateNoteRequest
{
  public const string Route = "/Notes";

  [Required]
  [MaxLength(200)]
  public string Title { get; set; } = String.Empty;

  [Required]
  [MaxLength(2000)]
  public string Content { get; set; } = String.Empty;
}

public class CreateNoteValidator : Validator<CreateNoteRequest>
{
  public CreateNoteValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .WithMessage("Title is required.")
      .MaximumLength(200)
      .WithMessage("Title must be less than 200 characters.");

    RuleFor(x => x.Content)
      .NotEmpty()
      .WithMessage("Content is required.")
      .MaximumLength(2000)
      .WithMessage("Content must be less than 2000 characters.");
  }
}

public class CreateNoteResponse(Guid id, string title, string content, DateTime createdOn)
{
  public Guid Id { get; set; } = id;
  public string Title { get; set; } = title;
  public string Content { get; set; } = content;
  public DateTime CreatedOn { get; set; } = createdOn;
}
