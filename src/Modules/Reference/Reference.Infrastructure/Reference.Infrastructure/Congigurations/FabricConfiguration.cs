using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Suppliers.Entities;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class FabricConfiguration : IEntityTypeConfiguration<Fabric>
{
    public void Configure(EntityTypeBuilder<Fabric> builder)
    {
        builder.ToTable("FabricsReference");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.FabricIdConvert)
            .ValueGeneratedNever();

        // The Calculator sheet has fabrics with the same name; the code is the unique key.
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL");

        builder.Property(x => x.Code).HasMaxLength(Fabric.CodeMaxLength);

        builder.Property(x => x.Name)
            .HasMaxLength(Fabric.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.NameRu).HasMaxLength(Fabric.NameMaxLength);
        builder.Property(x => x.Composition).HasMaxLength(Fabric.CompositionMaxLength);
        builder.Property(x => x.CompositionRu).HasMaxLength(Fabric.CompositionMaxLength);
        builder.Property(x => x.Density).HasMaxLength(Fabric.DensityMaxLength);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Comment).HasMaxLength(Fabric.CommentMaxLength);
        builder.Property(x => x.ProposalDescription).HasMaxLength(Fabric.ProposalDescriptionMaxLength);
        builder.Property(x => x.PriceUpdatedAt);

        builder.Property(x => x.Price)
            .HasConversion(ReferenceConverters.MoneyAmountConvert)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ProviderId)
            .HasConversion(ReferenceConverters.SupplierIdConvert)
            .IsRequired();

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .IsRequired();
    }
}

