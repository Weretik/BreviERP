using Production.Domain.Orders;
using Production.Infrastructure.Persistence;
using Production.Domain.Orders.Enums;
using Production.Domain.Orders.ValueObjects;


namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.HasIndex(x => x.Number).IsUnique();
        builder.Property(x => x.QuoteNumber).HasMaxLength(ProductionOrder.QuoteNumberMaxLength);
        builder.Property(x => x.ClientName).HasMaxLength(ProductionOrder.ClientMaxLength).IsRequired();
        builder.Property(x => x.ManagerName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PriceTier).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Comment).HasMaxLength(ProductionOrder.CommentMaxLength);
        builder.Property(x => x.LegacyId).HasMaxLength(ProductionOrder.LegacyIdMaxLength);
        builder.Property(x => x.CancelReason).HasMaxLength(ProductionOrder.CancelReasonMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.CreateRequestId).IsUnique().HasFilter("\"CreateRequestId\" IS NOT NULL");
        builder.HasIndex(x => x.LegacyId).IsUnique().HasFilter("\"LegacyId\" IS NOT NULL");
        builder.HasIndex(x => x.QuoteId);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.IsFullyShipped);

        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Changes).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Changes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Shipments).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Shipments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ProductionItemConfiguration : IEntityTypeConfiguration<ProductionItem>
{
    public void Configure(EntityTypeBuilder<ProductionItem> builder)
    {
        builder.ToTable("Items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.HasIndex(x => new { x.OrderId, x.Position }).IsUnique();
        builder.Property(x => x.ModelCode).HasMaxLength(ProductionItem.CodeMaxLength);
        builder.Property(x => x.ModelName).HasMaxLength(ProductionItem.NameMaxLength).IsRequired();
        builder.Property(x => x.FabricCode).HasMaxLength(ProductionItem.CodeMaxLength);
        builder.Property(x => x.FabricName).HasMaxLength(ProductionItem.NameMaxLength);
        builder.Property(x => x.SewingRate).HasPrecision(12, 2);
        builder.Property(x => x.ReportUnitPrice).HasPrecision(12, 2);
        builder.Property(x => x.PlannedFabricMeters).HasPrecision(12, 2);
        builder.Property(x => x.MainColor).HasMaxLength(ProductionItem.ColorMaxLength).IsRequired();
        builder.Property(x => x.SizesText).HasMaxLength(ProductionItem.LongTextMaxLength);
        builder.Property(x => x.TechnicalChanges).HasMaxLength(ProductionItem.LongTextMaxLength);
        builder.Property(x => x.Comment).HasMaxLength(ProductionItem.LongTextMaxLength);
        builder.Property<List<string>>("_additionalColors").HasColumnName("AdditionalColors").AsJson();
        builder.Property<List<ItemApplication>>("_applications").HasColumnName("Applications").AsJson();
        builder.Property<List<ProductionStage>>("_stages").HasColumnName("Stages").AsJson();
        builder.Ignore(x => x.Stages);
        builder.HasIndex(x => x.CurrentStage);
        builder.Ignore(x => x.AdditionalColors);
        builder.Ignore(x => x.Applications);
        builder.Ignore(x => x.ColorCount);
        builder.Ignore(x => x.Details);
        builder.Ignore(x => x.Source);
        builder.HasIndex(x => x.ModelCode);
    }
}

internal sealed class ProductionOrderChangeConfiguration : IEntityTypeConfiguration<ProductionOrderChange>
{
    public void Configure(EntityTypeBuilder<ProductionOrderChange> builder)
    {
        builder.ToTable("OrderChanges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.ItemName).HasMaxLength(ProductionItem.NameMaxLength);
        builder.Property(x => x.Field).HasMaxLength(ProductionOrderChange.FieldMaxLength).IsRequired();
        builder.Property(x => x.Before).HasMaxLength(ProductionOrderChange.ValueMaxLength);
        builder.Property(x => x.After).HasMaxLength(ProductionOrderChange.ValueMaxLength);
        builder.Property(x => x.ActorName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasIndex(x => x.OrderId);
    }
}

internal sealed class ProductionShipmentConfiguration : IEntityTypeConfiguration<ProductionShipment>
{
    public void Configure(EntityTypeBuilder<ProductionShipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Comment).HasMaxLength(ProductionShipment.CommentMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasOne<ProductionItem>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ItemId);
    }
}
