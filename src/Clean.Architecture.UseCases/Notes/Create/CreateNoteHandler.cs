using Ardalis.Result;
using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Create;

public class CreateNoteHandler(IRepository<Note> _repository)
  : ICommandHandler<CreateNoteCommand, Result<Guid>>
{
  public async ValueTask<Result<Guid>> Handle(CreateNoteCommand command, 
    CancellationToken cancellationToken)
  {
    var newNote = new Note(command.Title, command.Content);
    var createdItem = await _repository.AddAsync(newNote, cancellationToken);
    
    return createdItem.Id;
  }
}
