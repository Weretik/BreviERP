using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Stock;

namespace Warehouse.Infrastructure.Persistence.Configurations;

internal sealed class FabricStockConfiguration : IEntityTypeConfiguration<FabricStock>
{
    public void Configure(EntityTypeBuilder<FabricStock> builder)
    {
        builder.ToTable("FabricStocks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.FabricCode).HasMaxLength(FabricStock.CodeMaxLength).IsRequired();
        builder.Property(x => x.FabricName).HasMaxLength(FabricStock.NameMaxLength).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(StockColor.MaxLength).IsRequired();
        builder.Property(x => x.ColorKey).HasMaxLength(StockColor.MaxLength).IsRequired();
        builder.Property(x => x.Meters).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.LegacyId).HasMaxLength(FabricStock.LegacyIdMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasMany(x => x.Movements).WithOne(x => x.Stock).HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.FabricCode, x.ColorKey }).IsUnique();
        builder.HasIndex(x => x.LegacyId).IsUnique();
    }
}

internal sealed class FabricMovementConfiguration : IEntityTypeConfiguration<FabricMovement>
{
    public void Configure(EntityTypeBuilder<FabricMovement> builder)
    {
        builder.ToTable("FabricMovements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.MetersBefore).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.Delta).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.MetersAfter).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.Note).HasMaxLength(FabricMovement.NoteMaxLength);
        builder.Property(x => x.SupplierName).HasMaxLength(FabricMovement.SupplierMaxLength);
        builder.Property(x => x.Document).HasMaxLength(FabricMovement.DocumentMaxLength);
        builder.Property(x => x.ModelName).HasMaxLength(FabricMovement.ModelNameMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        builder.Property(x => x.LegacyId).HasMaxLength(FabricMovement.LegacyIdMaxLength);
        builder.HasOne<FabricMovement>().WithMany().HasForeignKey(x => x.ReversesMovementId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.StockId, x.Id });
        builder.HasIndex(x => x.OperationDate);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.LegacyId).IsUnique();
    }
}

internal sealed class FabricIssueConfiguration : IEntityTypeConfiguration<FabricIssue>
{
    public void Configure(EntityTypeBuilder<FabricIssue> builder)
    {
        builder.ToTable("FabricIssues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.ModelName).HasMaxLength(FabricMovement.ModelNameMaxLength).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(FabricIssue.NoteMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        builder.Property(x => x.ReversedByName).HasMaxLength(FabricMovement.PersonMaxLength);
        builder.Property(x => x.ReverseReason).HasMaxLength(FabricIssue.NoteMaxLength);
        builder.Property(x => x.LegacyId).HasMaxLength(FabricStock.LegacyIdMaxLength);
        builder.Ignore(x => x.Target);
        // OrderId and ItemId point to «Швейний цех» (Production) by value: no foreign key across modules.
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.IssueId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.LegacyId).IsUnique();
    }
}

internal sealed class FabricIssueLineConfiguration : IEntityTypeConfiguration<FabricIssueLine>
{
    public void Configure(EntityTypeBuilder<FabricIssueLine> builder)
    {
        builder.ToTable("FabricIssueLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PlanFabricCode).HasMaxLength(FabricStock.CodeMaxLength);
        builder.Property(x => x.PlanColorKey).HasMaxLength(StockColor.MaxLength);
        builder.Property(x => x.PlannedMeters).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.FabricCode).HasMaxLength(FabricStock.CodeMaxLength).IsRequired();
        builder.Property(x => x.FabricName).HasMaxLength(FabricStock.NameMaxLength).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(StockColor.MaxLength).IsRequired();
        builder.Property(x => x.Meters).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.CoveredMeters).HasPrecision(12, FabricStock.Decimals);
        builder.Ignore(x => x.Plan);
        builder.HasOne(x => x.Stock).WithMany().HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Movement).WithOne().HasForeignKey<FabricIssueLine>(x => x.MovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
