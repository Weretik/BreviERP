using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
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

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL");

        builder.Property(x => x.Code).HasMaxLength(GarmentAccessory.CodeMaxLength);

        builder.Property(x => x.Name)
            .HasMaxLength(GarmentAccessory.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(20)
            .HasDefaultValue(GarmentAccessory.DefaultUnit)
            .IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Comment).HasMaxLength(GarmentAccessory.CommentMaxLength);
        builder.Property(x => x.PriceUpdatedAt);

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
