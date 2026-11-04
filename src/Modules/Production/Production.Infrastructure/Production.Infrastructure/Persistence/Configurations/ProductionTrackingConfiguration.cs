using Production.Domain.Orders;
using Production.Domain.Settings;

namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class ProductionOrderFileConfiguration : IEntityTypeConfiguration<ProductionOrderFile>
{
    public void Configure(EntityTypeBuilder<ProductionOrderFile> builder)
    {
        builder.ToTable("OrderFiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Name).HasMaxLength(ProductionOrderFile.NameMaxLength).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(ProductionOrderFile.ContentTypeMaxLength).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(ProductionOrderFile.UrlMaxLength).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(ProductionOrderFile.StorageKeyMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasOne<ProductionOrder>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductionItem>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.OrderId);
    }
}

internal sealed class ProductionTelegramDeliveryConfiguration : IEntityTypeConfiguration<ProductionTelegramDelivery>
{
    public void Configure(EntityTypeBuilder<ProductionTelegramDelivery> builder)
    {
        builder.ToTable("TelegramDeliveries");
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.OrderId).ValueGeneratedNever();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.LastError).HasMaxLength(ProductionTelegramDelivery.ErrorMaxLength);
        builder.Property(x => x.SentByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.CanSend);
        builder.HasOne<ProductionOrder>().WithOne().HasForeignKey<ProductionTelegramDelivery>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class WorkshopSettingsConfiguration : IEntityTypeConfiguration<WorkshopSettings>
{
    public void Configure(EntityTypeBuilder<WorkshopSettings> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Ignore(x => x.CanSendTelegram);
    }
}
