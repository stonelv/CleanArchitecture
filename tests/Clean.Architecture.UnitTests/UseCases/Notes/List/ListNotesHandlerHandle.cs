using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes.List;
using Clean.Architecture.UseCases;
using Ardalis.SharedKernel;
using NSubstitute;
using Xunit;
using Shouldly;
using Ardalis.Specification;

namespace Clean.Architecture.UnitTests.UseCases.Notes.List;

public class ListNotesHandlerHandle
{
  private readonly IReadRepository<Note> _readRepository = Substitute.For<IReadRepository<Note>>();
  private ListNotesHandler _handler;

  public ListNotesHandlerHandle()
  {
    _handler = new ListNotesHandler(_readRepository);
  }

  private List<Note> CreateTestNotes()
  {
    return new List<Note>
    {
      new Note("Note 1", "Content for note 1"),
      new Note("Note 2", "Content for note 2"),
      new Note("Note 3", "Content for note 3")
    };
  }

  [Fact]
  public async Task ReturnsSuccessWithPagedResults()
  {
    var testNotes = CreateTestNotes();
    var pagedResult = new PagedResult<Note>(testNotes, 1, 10, 3, 1);

    _readRepository.ListAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(testNotes));
    _readRepository.CountAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(3));

    var result = await _handler.Handle(new ListNotesQuery(1, 10), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBeNull();
    result.Value.Items.Count.ShouldBe(3);
    result.Value.TotalCount.ShouldBe(3);
    result.Value.Page.ShouldBe(1);
    result.Value.PerPage.ShouldBe(10);
    result.Value.TotalPages.ShouldBe(1);
  }

  [Fact]
  public async Task ReturnsSuccessWithEmptyResultsWhenNoNotesExist()
  {
    _readRepository.ListAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(new List<Note>()));
    _readRepository.CountAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(0));

    var result = await _handler.Handle(new ListNotesQuery(1, 10), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBeNull();
    result.Value.Items.ShouldBeEmpty();
    result.Value.TotalCount.ShouldBe(0);
    result.Value.Page.ShouldBe(1);
    result.Value.PerPage.ShouldBe(10);
    result.Value.TotalPages.ShouldBe(0);
  }
}
