using Ardalis.SharedKernel;

namespace Clean.Architecture.Core.NoteAggregate;

public class Note : EntityBase<Note, Guid>, IAggregateRoot
{
  private Note() 
  {
    Title = string.Empty;
    Content = string.Empty;
    CreatedOn = DateTime.Now;
  }

  public Note(string title, string content)
  {
    Title = title;
    Content = content;
    CreatedOn = DateTime.Now;
  }

  public string Title { get; private set; }
  public string Content { get; private set; }
  public DateTime CreatedOn { get; private set; }

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
