namespace Catalog.Infrastructure.Pricing.History;

internal sealed class SewingPricePublicationConfiguration : IEntityTypeConfiguration<SewingPricePublicationRecord>
{
    public void Configure(EntityTypeBuilder<SewingPricePublicationRecord> builder)
    {
        builder.ToTable("SewingPricePublications", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.CalculationId).IsUnique();
        builder.Property(x => x.PublishedBy).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.Calculation).WithMany().HasForeignKey(x => x.CalculationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
