using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Products.Entities;
using Reference.Domain.Suppliers.Entities;
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

        builder.Property(x => x.SupplierId)
            .HasConversion(ReferenceConverters.SupplierIdConvert)
            .HasDefaultValueSql("1")
            .IsRequired();

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .IsRequired();
    }
}
