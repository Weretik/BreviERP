using Production.Domain.Archive;

namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class WorkshopArchiveConfiguration : IEntityTypeConfiguration<WorkshopArchiveRecord>
{
    public void Configure(EntityTypeBuilder<WorkshopArchiveRecord> builder)
    {
        builder.ToTable("ArchiveRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.SourceKey).HasMaxLength(WorkshopArchiveRecord.SourceKeyMaxLength).IsRequired();
        builder.Property(x => x.DateText).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Number).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Client).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Product).HasMaxLength(WorkshopArchiveRecord.TextMaxLength);
        builder.Property(x => x.Quantity).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Fabric).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Status).HasMaxLength(WorkshopArchiveRecord.ShortMaxLength);
        builder.Property(x => x.Notes).HasMaxLength(WorkshopArchiveRecord.TextMaxLength);
        builder.Property(x => x.SearchText).HasMaxLength(WorkshopArchiveRecord.SearchMaxLength).IsRequired();
        builder.Ignore(x => x.Fields);
        builder.Ignore(x => x.Links);
        builder.Property<List<WorkshopArchiveField>>("_fields").HasColumnName("Fields").AsJson();
        builder.Property<List<WorkshopArchiveLink>>("_links").HasColumnName("Links").AsJson();
        builder.HasIndex(x => x.SourceKey).IsUnique();
        builder.HasIndex(x => new { x.Kind, x.Date });
    }
}
