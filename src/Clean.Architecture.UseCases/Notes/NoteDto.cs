namespace Clean.Architecture.UseCases.Notes;

public record NoteDto(Guid Id, string Title, string Content, DateTime CreatedOn);
