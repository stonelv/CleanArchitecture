using Ardalis.Result;

namespace Clean.Architecture.UseCases.Notes.Delete;

public record DeleteNoteCommand(Guid Id) : ICommand<Result>;
