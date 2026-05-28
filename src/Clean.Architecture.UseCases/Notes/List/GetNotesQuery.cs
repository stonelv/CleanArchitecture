namespace Clean.Architecture.UseCases.Notes.List;

/// <summary>
/// Get all Notes.
/// </summary>
public record GetNotesQuery : IQuery<Result<IEnumerable<NoteDto>>>;