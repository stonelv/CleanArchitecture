using System.ComponentModel.DataAnnotations;
using Clean.Architecture.UseCases.Notes.Update;
using Clean.Architecture.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Clean.Architecture.Web.Notes;

public class Update(IMediator mediator)
  : Endpoint<UpdateNoteRequest, 
          Results<Ok<UpdateNoteResponse>, 
                  NotFound, 
                  ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Put(UpdateNoteRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Update a note";
      s.Description = "Updates a note with the provided id, title and content.";
      s.ExampleRequest = new UpdateNoteRequest { Id = Guid.NewGuid(), Title = "Updated Note", Content = "This is an updated note." };
      s.ResponseExamples[200] = new UpdateNoteResponse(Guid.NewGuid(), "Updated Note", "This is an updated note.", DateTime.Now);

      // Document possible responses
      s.Responses[200] = "Note updated successfully";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[404] = "Note not found";
      s.Responses[500] = "Internal server error";
    });

    // Add tags for API grouping
    Tags("Notes");

    // Add additional metadata
    Description(builder => builder
      .Accepts<UpdateNoteRequest>("application/json")
      .Produces<UpdateNoteResponse>(200, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(404)
      .ProducesProblem(500));
  }

  public override async Task<Results<Ok<UpdateNoteResponse>, NotFound, ProblemHttpResult>>
    ExecuteAsync(UpdateNoteRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateNoteCommand(request.Id, request.Title!, request.Content!));

    return result.ToUpdateResult(
      _ => new UpdateNoteResponse(request.Id, request.Title!, request.Content!, DateTime.Now));
  }
}

public class UpdateNoteRequest
{
  public const string Route = "/Notes/{Id}";

  [Required]
  public Guid Id { get; set; }

  [Required]
  [MaxLength(200)]
  public string Title { get; set; } = String.Empty;

  [Required]
  [MaxLength(2000)]
  public string Content { get; set; } = String.Empty;
}

public class UpdateNoteValidator : Validator<UpdateNoteRequest>
{
  public UpdateNoteValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("Id is required.");

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

public class UpdateNoteResponse(Guid id, string title, string content, DateTime updatedOn)
{
  public Guid Id { get; set; } = id;
  public string Title { get; set; } = title;
  public string Content { get; set; } = content;
  public DateTime UpdatedOn { get; set; } = updatedOn;
}
