using Ardalis.Result;
using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes;
using Clean.Architecture.UseCases;

namespace Clean.Architecture.UseCases.Notes.List;

public class ListNotesHandler(IReadRepository<Note> _repository)
  : IQueryHandler<ListNotesQuery, Result<PagedResult<NoteDto>>> 
{
  public async ValueTask<Result<PagedResult<NoteDto>>> Handle(ListNotesQuery request, 
    CancellationToken cancellationToken)
  {
    var page = request.Page ?? 1;
    var perPage = request.PerPage ?? 10;
    
    // Get all notes and sort them by created date (descending)
    var allNotes = await _repository.ListAsync(cancellationToken);
    var sortedNotes = allNotes.OrderByDescending(n => n.CreatedOn).ToList();
    
    // Calculate pagination
    var totalCount = sortedNotes.Count;
    var totalPages = (int)Math.Ceiling((double)totalCount / perPage);
    var paginatedNotes = sortedNotes.Skip((page - 1) * perPage).Take(perPage).ToList();
    
    // Map to NoteDto
    var noteDtos = paginatedNotes.Select(n => new NoteDto(n.Id, n.Title, n.Content, n.CreatedOn)).ToList();
    
    // Create PagedResult with correct parameter order
    var pagedResult = new PagedResult<NoteDto>(noteDtos, page, perPage, totalCount, totalPages);
    
    return Result.Success(pagedResult);
  }
}

