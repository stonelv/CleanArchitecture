using Ardalis.Result;

namespace Clean.Architecture.UseCases.Notes.List;

public record ListNotesQuery(int? Page = 1, int? PerPage = Constants.DEFAULT_PAGE_SIZE) 
  : IQuery<Result<PagedResult<NoteDto>>>;
