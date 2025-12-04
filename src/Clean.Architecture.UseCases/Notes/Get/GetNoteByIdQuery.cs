using Ardalis.Result;

namespace Clean.Architecture.UseCases.Notes.Get;

public record GetNoteByIdQuery(Guid Id) : IQuery<Result<NoteDto>>;
