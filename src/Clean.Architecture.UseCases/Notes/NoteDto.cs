using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.UseCases.Notes;
public record NoteDto(NoteId Id, string Title, string Content, DateTime CreatedOn);