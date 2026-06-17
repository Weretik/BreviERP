using Reference.Domain.Entities;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

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
