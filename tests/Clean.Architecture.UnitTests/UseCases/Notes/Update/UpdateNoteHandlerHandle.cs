using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Update;
using Ardalis.SharedKernel;
using NSubstitute;
using Xunit;
using Shouldly;

namespace Clean.Architecture.UnitTests.UseCases.Notes.Update;

public class UpdateNoteHandlerHandle
{
  private readonly Guid _testNoteId = Guid.NewGuid();
  private readonly string _originalTitle = "Original Note Title";
  private readonly string _originalContent = "This is the original content of the test note.";
  private readonly string _updatedTitle = "Updated Note Title";
  private readonly string _updatedContent = "This is the updated content of the test note.";
  private readonly IRepository<Note> _repository = Substitute.For<IRepository<Note>>();
  private UpdateNoteHandler _handler;

  public UpdateNoteHandlerHandle()
  {
    _handler = new UpdateNoteHandler(_repository);
  }

  private Note CreateOriginalNote()
  {
    return new Note(_originalTitle, _originalContent);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidNoteIdAndUpdatedData()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(CreateOriginalNote()));

    var result = await _handler.Handle(new UpdateNoteCommand(_testNoteId, _updatedTitle, _updatedContent), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    await _repository.Received(1).UpdateAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsFailureGivenNonExistentNoteId()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(null));

    var result = await _handler.Handle(new UpdateNoteCommand(_testNoteId, _updatedTitle, _updatedContent), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsFailureGivenEmptyTitle()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(CreateOriginalNote()));

    var result = await _handler.Handle(new UpdateNoteCommand(_testNoteId, string.Empty, _updatedContent), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ReturnsFailureGivenEmptyContent()
  {
    _repository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(CreateOriginalNote()));

    var result = await _handler.Handle(new UpdateNoteCommand(_testNoteId, _updatedTitle, string.Empty), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>());
  }
}
