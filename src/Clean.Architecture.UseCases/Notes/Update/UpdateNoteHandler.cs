using Ardalis.Result;
using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Update;

public class UpdateNoteHandler(IRepository<Note> _repository)
  : ICommandHandler<UpdateNoteCommand, Result>
{
  public async ValueTask<Result> Handle(UpdateNoteCommand command, 
    CancellationToken cancellationToken)
  {
    var note = await _repository.GetByIdAsync(command.Id, cancellationToken);
    if (note is null)
    {
      return Result.NotFound();
    }
    
    note.UpdateTitle(command.Title);
    note.UpdateContent(command.Content);
    
    await _repository.UpdateAsync(note, cancellationToken);
    
    return Result.Success();
  }
}
