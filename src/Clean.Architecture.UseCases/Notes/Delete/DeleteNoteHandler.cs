using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Delete;

public class DeleteNoteHandler(IRepository<Note> _repository)
  : ICommandHandler<DeleteNoteCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteNoteCommand command, CancellationToken cancellationToken)
  {
    var note = await _repository.GetByIdAsync(command.NoteId, cancellationToken);
    if (note == null)
    {
      return Result.NotFound();
    }

    await _repository.DeleteAsync(note, cancellationToken);
    return Result.Success();
  }
}