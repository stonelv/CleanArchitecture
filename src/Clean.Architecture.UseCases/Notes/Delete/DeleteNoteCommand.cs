using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Delete;

/// <summary>
/// Delete a Note.
/// </summary>
/// <param name="NoteId"></param>
public record DeleteNoteCommand(NoteId NoteId) : ICommand<Result>;