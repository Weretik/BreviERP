using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.Configuration;

public sealed class ReferencePricingInputVersionConfiguration : IEntityTypeConfiguration<ReferencePricingInputVersion>
{
    public void Configure(EntityTypeBuilder<ReferencePricingInputVersion> builder)
    {
        builder.ToTable("PricingInputVersions", "reference", table =>
        {
            table.HasCheckConstraint("CK_PricingInputVersions_OneReference",
                "num_nonnulls(\"FabricId\", \"AccessoryId\", \"OperationId\", \"OtherCostCode\") = 1");
            table.HasCheckConstraint("CK_PricingInputVersions_NonNegative", "\"Value\" IS NULL OR \"Value\" >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => ReferencePricingInputVersionId.Create(x))
            .ValueGeneratedOnAdd().HasValueGenerator<ReferencePricingInputVersionIdGenerator>();
        builder.Property(x => x.FabricId).HasConversion(new ValueConverter<FabricId?, int?>(
            x => x.HasValue ? x.Value.Value : null, x => x.HasValue ? FabricId.From(x.Value) : null));
        builder.Property(x => x.AccessoryId).HasConversion(new ValueConverter<GarmentAccessoryId?, int?>(
            x => x.HasValue ? x.Value.Value : null, x => x.HasValue ? GarmentAccessoryId.From(x.Value) : null));
        builder.Property(x => x.OperationId).HasConversion(new ValueConverter<GarmentPartOperationId?, int?>(
            x => x.HasValue ? x.Value.Value : null, x => x.HasValue ? GarmentPartOperationId.From(x.Value) : null));
        builder.Property(x => x.OtherCostCode).HasMaxLength(80);
        builder.Property(x => x.Value).HasPrecision(20, 8);
        builder.Property(x => x.SourceCode).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SourceDate).HasMaxLength(100);
        builder.HasIndex(x => new { x.SourceCode, x.Revision }).IsUnique();
        builder.HasIndex(x => new { x.SourceHash, x.SourceCode }).IsUnique();
        builder.HasIndex(x => new { x.FabricId, x.EffectiveFrom });
        builder.HasIndex(x => new { x.AccessoryId, x.EffectiveFrom });
        builder.HasIndex(x => new { x.OperationId, x.EffectiveFrom });
        builder.HasIndex(x => new { x.OtherCostCode, x.EffectiveFrom });
        builder.HasOne<Fabric>().WithMany().HasForeignKey(x => x.FabricId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GarmentAccessory>().WithMany().HasForeignKey(x => x.AccessoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GarmentPartOperation>().WithMany().HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Restrict);
    }
}

