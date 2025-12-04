using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes.Get;

/// <summary>
/// Get a Note by Id.
/// </summary>
/// <param name="NoteId"></param>
public record GetNoteQuery(NoteId NoteId) : IQuery<Result<NoteDto>>;