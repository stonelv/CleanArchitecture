using Ardalis.Result;

namespace Clean.Architecture.UseCases.Notes.Update;

public record UpdateNoteCommand(Guid Id, string Title, string Content) : ICommand<Result>;
