using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Create;

/// <summary>
/// Create a new Note.
/// </summary>
/// <param name="Title"></param>
/// <param name="Content"></param>
public record CreateNoteCommand(string Title, string Content) : ICommand<Result<NoteId>>;