using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Delete;
using Ardalis.SharedKernel;
using NSubstitute;
using Xunit;
using Shouldly;

namespace Clean.Architecture.UnitTests.UseCases.Notes.Delete;

public class DeleteNoteHandlerHandle
{
  private readonly Guid _testNoteId = Guid.NewGuid();
  private readonly string _testTitle = "Test Note Title";
  private readonly string _testContent = "This is the content of the test note.";
  private readonly IRepository<Note> _repository = Substitute.For<IRepository<Note>>();
  private DeleteNoteHandler _handler;

  public DeleteNoteHandlerHandle()
  {
    _handler = new DeleteNoteHandler(_repository);
  }

  private Note CreateNote()
  {
    return new Note(_testTitle, _testContent);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidNoteId()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(CreateNote()));

    var result = await _handler.Handle(new DeleteNoteCommand(_testNoteId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    await _repository.Received(1).DeleteAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsFailureGivenNonExistentNoteId()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(null));

    var result = await _handler.Handle(new DeleteNoteCommand(_testNoteId), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    await _repository.DidNotReceive().DeleteAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }
}
