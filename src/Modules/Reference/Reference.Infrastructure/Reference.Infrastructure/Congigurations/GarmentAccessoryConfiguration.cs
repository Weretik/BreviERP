using Reference.Domain.Entities;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class GarmentAccessoryConfiguration : IEntityTypeConfiguration<GarmentAccessory>
{
    public void Configure(EntityTypeBuilder<GarmentAccessory> builder)
    {
        builder.ToTable("GarmentAccessoriesReference");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.GarmentAccessoryIdConvert)
            .ValueGeneratedNever();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(ReferenceConverters.MoneyAmountConvert)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
