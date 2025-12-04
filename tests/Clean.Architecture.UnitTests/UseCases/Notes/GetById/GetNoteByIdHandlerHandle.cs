using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.Get;
using Ardalis.SharedKernel;
using NSubstitute;
using Xunit;
using Shouldly;

namespace Clean.Architecture.UnitTests.UseCases.Notes.GetById;

public class GetNoteByIdHandlerHandle
{
  private readonly Guid _testNoteId = Guid.NewGuid();
  private readonly string _testTitle = "Test Note Title";
  private readonly string _testContent = "This is the content of the test note.";
  private readonly IReadRepository<Note> _readRepository = Substitute.For<IReadRepository<Note>>();
  private GetNoteByIdHandler _handler;

  public GetNoteByIdHandlerHandle()
  {
    _handler = new GetNoteByIdHandler(_readRepository);
  }

  private Note CreateNote()
  {
    return new Note(_testTitle, _testContent);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidNoteId()
  {
    _readRepository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(CreateNote()));
    var result = await _handler.Handle(new GetNoteByIdQuery(_testNoteId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBeNull();
    result.Value.Title.ShouldBe(_testTitle);
    result.Value.Content.ShouldBe(_testContent);
  }

  [Fact]
  public async Task ReturnsFailureGivenNonExistentNoteId()
  {
    _readRepository.GetByIdAsync(_testNoteId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<Note?>(null));
    var result = await _handler.Handle(new GetNoteByIdQuery(_testNoteId), CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
  }
}
