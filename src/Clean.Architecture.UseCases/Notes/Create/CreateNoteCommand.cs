using Ardalis.Result;

namespace Clean.Architecture.UseCases.Notes.Create;

public record CreateNoteCommand(string Title, string Content) : ICommand<Result<Guid>>;
