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

        builder.Property(x => x.SupplierId)
            .HasConversion(ReferenceConverters.SupplierIdConvert)
            .HasDefaultValueSql("1")
            .IsRequired();

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(200);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .IsRequired();
    }
}
