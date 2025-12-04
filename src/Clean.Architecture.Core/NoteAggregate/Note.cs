namespace Clean.Architecture.Core.NoteAggregate;

public class Note : EntityBase<Note, NoteId>, IAggregateRoot
{
  public string Title { get; private set; }
  public string Content { get; private set; }
  public DateTime CreatedOn { get; private set; }

  public Note(string title, string content)
  {
    Title = title;
    Content = content;
    CreatedOn = DateTime.UtcNow;
  }

  public Note UpdateTitle(string newTitle)
  {
    if (Title == newTitle) return this;
    Title = newTitle;
    return this;
  }

  public Note UpdateContent(string newContent)
  {
    if (Content == newContent) return this;
    Content = newContent;
    return this;
  }
}