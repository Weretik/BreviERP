using Reference.Domain.Entities;
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

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Min)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
