using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Suppliers.Entities;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class GarmentPartOperationConfiguration : IEntityTypeConfiguration<GarmentPartOperation>
{
    public void Configure(EntityTypeBuilder<GarmentPartOperation> builder)
    {
        builder.ToTable("GarmentPartOperations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.GarmentPartOperationIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.GarmentPartId)
            .HasConversion(ReferenceConverters.GarmentPartIdConvert)
            .IsRequired();

        builder.HasIndex(x => new { x.GarmentPartId, x.Name })
            .IsUnique();

        builder.Property(x => x.Code)
            .HasMaxLength(GarmentPartOperation.CodeMaxLength);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL");

        builder.Property(x => x.Element)
            .HasMaxLength(GarmentPartOperation.ElementMaxLength);

        builder.Property(x => x.Variant)
            .HasMaxLength(GarmentPartOperation.VariantMaxLength);

        builder.Property(x => x.Name)
            .HasMaxLength(GarmentPartOperation.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.MinutesPerProduct)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(GarmentPartOperation.NotesMaxLength);

        builder.Property(x => x.Min)
            .HasPrecision(24, 8)
            .IsRequired();
    }
}
