using Ardalis.Result;
using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes;

namespace Clean.Architecture.UseCases.Notes.Get;

public class GetNoteByIdHandler(IReadRepository<Note> _repository)
  : IQueryHandler<GetNoteByIdQuery, Result<NoteDto>>
{
  public async ValueTask<Result<NoteDto>> Handle(GetNoteByIdQuery query, 
    CancellationToken cancellationToken)
  {
    var note = await _repository.GetByIdAsync(query.Id, cancellationToken);
    if (note is null)
    {
      return Result.NotFound();
    }
    
    var noteDto = new NoteDto(note.Id, note.Title, note.Content, note.CreatedOn);
    
    return noteDto;
  }
}
