using Vogen;

namespace Clean.Architecture.Core.NoteAggregate;

[ValueObject<Guid>]
public readonly partial struct NoteId
{
  private static Validation Validate(Guid value) =>
    value != Guid.Empty ? Validation.Ok : Validation.Invalid("NoteId cannot be empty.");
}