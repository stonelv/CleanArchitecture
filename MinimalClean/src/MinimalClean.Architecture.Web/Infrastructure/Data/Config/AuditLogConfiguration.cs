using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinimalClean.Architecture.Web.Domain.AuditLogAggregate;

namespace MinimalClean.Architecture.Web.Infrastructure.Data.Config;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(entity => entity.Id)
            .HasValueGenerator<VogenGuidIdValueGenerator<AppDbContext, AuditLog, AuditLogId>>()
            .HasVogenConversion()
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasMaxLength(255);

        builder.Property(entity => entity.Endpoint)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entity => entity.HttpMethod)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(entity => entity.ElapsedMilliseconds)
            .IsRequired();

        builder.Property(entity => entity.StatusCode);

        builder.Property(entity => entity.IsSuccess)
            .IsRequired();

        builder.Property(entity => entity.ExceptionMessage)
            .HasMaxLength(2000);

        builder.Property(entity => entity.ExceptionStackTrace)
            .HasMaxLength(4000);

        builder.Property(entity => entity.ClientIp)
            .HasMaxLength(45);

        builder.Property(entity => entity.UserAgent)
            .HasMaxLength(1000);

        builder.Property(entity => entity.RequestPath)
            .HasMaxLength(2000);

        builder.Property(entity => entity.RequestMethod)
            .HasMaxLength(10);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.CreatedAt);
        builder.HasIndex(entity => entity.Endpoint);
        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => entity.IsSuccess);
    }
}
