using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes;

namespace Clean.Architecture.UseCases.Notes.Get;

public class GetNoteHandler(IReadRepository<Note> _repository)
  : IQueryHandler<GetNoteQuery, Result<NoteDto>>
{
  public async ValueTask<Result<NoteDto>> Handle(GetNoteQuery query, CancellationToken cancellationToken)
  {
    var note = await _repository.GetByIdAsync(query.NoteId, cancellationToken);
    if (note == null)
    {
      return Result.NotFound();
    }

    var noteDto = new NoteDto(note.Id, note.Title, note.Content, note.CreatedOn);
    return noteDto;
  }
}