using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Update;

/// <summary>
/// Update a Note.
/// </summary>
/// <param name="NoteId"></param>
/// <param name="Title"></param>
/// <param name="Content"></param>
public record UpdateNoteCommand(NoteId NoteId, string Title, string Content) : ICommand<Result<NoteDto>>;