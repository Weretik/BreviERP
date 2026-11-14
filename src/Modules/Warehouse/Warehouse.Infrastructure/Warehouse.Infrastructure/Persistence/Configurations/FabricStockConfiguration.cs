using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Domain.Stock.Plans;


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
        builder.Property(x => x.Destination).HasMaxLength(FabricMovement.FormTextMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        builder.Property(x => x.LegacyId).HasMaxLength(FabricMovement.LegacyIdMaxLength);
        builder.HasOne<FabricMovement>().WithMany().HasForeignKey(x => x.ReversesMovementId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.IsVoided).HasDefaultValue(false);
        builder.Property(x => x.Revision).HasDefaultValue(0).IsConcurrencyToken();
        builder.HasIndex(x => new { x.StockId, x.Id });
        builder.HasIndex(x => x.OperationDate);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.LegacyId).IsUnique();
        builder.HasIndex(x => x.OperationId);
        builder.HasIndex(x => x.IssueLineId);
    }
}

internal sealed class FabricMovementRevisionConfiguration : IEntityTypeConfiguration<FabricMovementRevision>
{
    public void Configure(EntityTypeBuilder<FabricMovementRevision> builder)
    {
        builder.ToTable("FabricMovementRevisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.BeforeDelta).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.AfterDelta).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.BeforeSupplierName).HasMaxLength(FabricMovement.SupplierMaxLength);
        builder.Property(x => x.AfterSupplierName).HasMaxLength(FabricMovement.SupplierMaxLength);
        builder.Property(x => x.BeforeDocument).HasMaxLength(FabricMovement.DocumentMaxLength);
        builder.Property(x => x.AfterDocument).HasMaxLength(FabricMovement.DocumentMaxLength);
        builder.Property(x => x.BeforeDestination).HasMaxLength(FabricMovement.FormTextMaxLength);
        builder.Property(x => x.AfterDestination).HasMaxLength(FabricMovement.FormTextMaxLength);
        builder.Property(x => x.Reason).HasMaxLength(FabricMovementRevision.ReasonMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        builder.Ignore(x => x.Before);
        builder.Ignore(x => x.After);
        builder.HasOne(x => x.Movement).WithMany().HasForeignKey(x => x.MovementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AfterStock).WithMany().HasForeignKey(x => x.AfterStockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FabricStock>().WithMany().HasForeignKey(x => x.BeforeStockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FabricMovementRevision>().WithMany().HasForeignKey(x => x.RestoresRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MovementId, x.Number }).IsUnique();
    }
}

internal sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("StockOperations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Kind).HasMaxLength(StockOperation.KindMaxLength).IsRequired();
        builder.Property(x => x.RequestHash).HasMaxLength(StockOperation.HashLength).IsRequired();
        builder.Property(x => x.FabricCode).HasMaxLength(FabricStock.CodeMaxLength).IsRequired();
        builder.Property(x => x.ColorKey).HasMaxLength(StockColor.MaxLength).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(StockColor.MaxLength).IsRequired();
        builder.Property(x => x.MetersAfter).HasPrecision(14, FabricStock.Decimals);
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
        builder.HasIndex(x => x.OperationId).IsUnique();
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
        builder.Property(x => x.PlanCoverage).HasPrecision(12, FabricStock.Decimals);
        builder.Property(x => x.Layer).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Note).HasMaxLength(FabricIssueLine.NoteMaxLength);
        builder.Ignore(x => x.Plan);
        builder.Ignore(x => x.IsVoided);
        builder.HasOne(x => x.Stock).WithMany().HasForeignKey(x => x.StockId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Movements).WithOne().HasForeignKey(x => x.IssueLineId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class FabricItemPlanConfiguration : IEntityTypeConfiguration<FabricItemPlan>
{
    public void Configure(EntityTypeBuilder<FabricItemPlan> builder)
    {
        builder.ToTable("FabricItemPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property(x => x.UpdatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        // OrderId and ItemId point to «Швейний цех» (Production) by value: no foreign key across modules.
        builder.HasIndex(x => x.ItemId).IsUnique();
        builder.OwnsMany(x => x.Parts, part =>
        {
            part.ToTable("FabricItemPlanParts");
            part.WithOwner().HasForeignKey("PlanId");
            part.Property<int>("Id").UseIdentityByDefaultColumn();
            part.HasKey("Id");
            part.Property(x => x.FabricCode).HasMaxLength(FabricStock.CodeMaxLength).IsRequired();
            part.Property(x => x.Color).HasMaxLength(StockColor.MaxLength).IsRequired();
            part.Property(x => x.ColorKey).HasMaxLength(StockColor.MaxLength).IsRequired();
            part.Property(x => x.Layer).HasConversion<string>().HasMaxLength(20);
            part.Property(x => x.Meters).HasPrecision(12, FabricStock.Decimals);
            part.Ignore(x => x.Key);
        });
        builder.Navigation(x => x.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Hash).HasMaxLength(StockTransfer.HashLength).IsFixedLength().IsRequired();
        builder.Property(x => x.Snapshot).HasMaxLength(StockTransfer.SnapshotMaxLength);
        builder.Property(x => x.Meters).HasPrecision(14, FabricStock.Decimals);
        builder.Property(x => x.CreatedByName).HasMaxLength(FabricMovement.PersonMaxLength).IsRequired();
        builder.HasIndex(x => x.Hash);
    }
}
