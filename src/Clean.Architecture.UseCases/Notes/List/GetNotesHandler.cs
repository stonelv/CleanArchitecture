using Clean.Architecture.Core.NoteAggregate;
using Clean.Architecture.UseCases.Notes;

namespace Clean.Architecture.UseCases.Notes.List;

public class GetNotesHandler(IReadRepository<Note> _repository)
  : IQueryHandler<GetNotesQuery, Result<IEnumerable<NoteDto>>> 
{
  public async ValueTask<Result<IEnumerable<NoteDto>>> Handle(GetNotesQuery query, CancellationToken cancellationToken)
  {
    var notes = await _repository.ListAsync(cancellationToken);
    var noteDtos = notes.Select(n => new NoteDto(n.Id, n.Title, n.Content, n.CreatedOn));
    return Result.Success(noteDtos);
  }
}