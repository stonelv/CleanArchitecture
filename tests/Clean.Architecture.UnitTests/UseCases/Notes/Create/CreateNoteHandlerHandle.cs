using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Create;
using Ardalis.SharedKernel;
using NSubstitute;
using Xunit;
using Shouldly;

namespace Clean.Architecture.UnitTests.UseCases.Notes.Create;

public class CreateNoteHandlerHandle
{
  private readonly string _testTitle = "Test Note Title";
  private readonly string _testContent = "This is the content of the test note.";
  private readonly IRepository<Note> _repository = Substitute.For<IRepository<Note>>();
  private CreateNoteHandler _handler;

  public CreateNoteHandlerHandle()
  {
    _handler = new CreateNoteHandler(_repository);
  }

  private Note CreateNote()
  {
    return new Note(_testTitle, _testContent);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidTitleAndContent()
  {
    _repository.AddAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(CreateNote()));
    var result = await _handler.Handle(new CreateNoteCommand(_testTitle, _testContent), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBe(Guid.Empty);
  }

  [Fact]
  public async Task ReturnsFailureGivenEmptyTitle()
  {
    var result = await _handler.Handle(new CreateNoteCommand(string.Empty, _testContent), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
  }

  [Fact]
  public async Task ReturnsFailureGivenEmptyContent()
  {
    var result = await _handler.Handle(new CreateNoteCommand(_testTitle, string.Empty), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
  }
}
