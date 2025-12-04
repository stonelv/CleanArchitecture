using Clean.Architecture.Core.NoteAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Architecture.Infrastructure.Data.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
  public void Configure(EntityTypeBuilder<Note> builder)
  {
    builder.Property(n => n.Title)
      .IsRequired()
      .HasMaxLength(200);

    builder.Property(n => n.Content)
      .IsRequired();

    builder.Property(n => n.CreatedOn)
      .IsRequired();
  }
}