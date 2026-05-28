using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes;

namespace Clean.Architecture.UseCases.Notes.Update;

public class UpdateNoteHandler(IRepository<Note> _repository)
  : ICommandHandler<UpdateNoteCommand, Result<NoteDto>>
{
  public async ValueTask<Result<NoteDto>> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
  {
    var note = await _repository.GetByIdAsync(command.NoteId, cancellationToken);
    if (note == null)
    {
      return Result.NotFound();
    }

    note.UpdateTitle(command.Title);
    note.UpdateContent(command.Content);
    await _repository.UpdateAsync(note, cancellationToken);

    var noteDto = new NoteDto(note.Id, note.Title, note.Content, note.CreatedOn);
    return noteDto;
  }
}