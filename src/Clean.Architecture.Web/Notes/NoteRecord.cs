namespace Clean.Architecture.Web.Notes;

public record NoteRecord(Guid Id, string Title, string Content, DateTime CreatedOn);