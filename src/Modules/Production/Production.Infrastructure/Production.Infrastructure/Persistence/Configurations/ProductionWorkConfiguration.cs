using Production.Domain.Forecast;
using Production.Domain.Orders;
using Production.Domain.Staff;
using Production.Domain.Work;

namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class ProductionWorkConfiguration : IEntityTypeConfiguration<ProductionWork>
{
    public void Configure(EntityTypeBuilder<ProductionWork> builder)
    {
        builder.ToTable("Work");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.EmployeeName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(ProductionWork.ReasonMaxLength);
        builder.Property(x => x.Comment).HasMaxLength(ProductionWork.CommentMaxLength);
        builder.Property(x => x.Rate).HasPrecision(12, 2);
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.VoidedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.VoidReason).HasMaxLength(ProductionWork.ReasonMaxLength);
        builder.Property(x => x.LegacyId).HasMaxLength(ProductionWork.LegacyIdMaxLength);
        builder.Ignore(x => x.IsVoided);
        builder.Ignore(x => x.CountsToPlan);
        builder.HasOne<ProductionOrder>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductionItem>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WorkDate);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.LegacyId).IsUnique();
    }
}

internal sealed class ForecastQueueConfiguration : IEntityTypeConfiguration<ForecastQueue>
{
    public void Configure(EntityTypeBuilder<ForecastQueue> builder)
    {
        builder.ToTable("ForecastQueues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.OrderIds).AsJson();
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
