using Accounting.Domain.Audit;

namespace Accounting.Infrastructure.Persistence.Audit;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Entity).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.By).HasMaxLength(AuditEntry.ByMaxLength).IsRequired();
        builder.Property(x => x.BeforeJson).HasColumnType("jsonb");
        builder.Property(x => x.AfterJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.Entity, x.EntityId, x.Id });
        builder.HasIndex(x => x.At);
    }
}
