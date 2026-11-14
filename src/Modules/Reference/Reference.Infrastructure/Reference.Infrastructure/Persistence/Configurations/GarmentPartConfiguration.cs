using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Infrastructure.Persistence;
using Reference.Domain.GarmentParts;


namespace Reference.Infrastructure.Persistence.Configurations;

public sealed class GarmentPartConfiguration : IEntityTypeConfiguration<GarmentPart>
{
    public void Configure(EntityTypeBuilder<GarmentPart> builder)
    {
        builder.ToTable("GarmentParts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.GarmentPartIdConvert)
            .ValueGeneratedNever();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();
    }
}
