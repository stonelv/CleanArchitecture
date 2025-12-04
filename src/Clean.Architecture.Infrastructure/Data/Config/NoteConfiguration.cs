using Clean.Architecture.Core.NoteAggregate;

namespace Clean.Architecture.Infrastructure.Data.Config;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
  public void Configure(EntityTypeBuilder<Note> builder)
  {
    builder.Property(entity => entity.Id)
      .IsRequired();

    builder.Property(entity => entity.Title)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(entity => entity.Content)
      .HasMaxLength(2000)
      .IsRequired();

    builder.Property(entity => entity.CreatedOn)
      .IsRequired();
  }
}
