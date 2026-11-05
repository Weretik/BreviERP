using Production.Domain.Orders;
using Production.Domain.Payroll;
using Production.Domain.Staff;
using Production.Domain.Timesheet;

namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class WorkshopTimeEntryConfiguration : IEntityTypeConfiguration<WorkshopTimeEntry>
{
    public void Configure(EntityTypeBuilder<WorkshopTimeEntry> builder)
    {
        builder.ToTable("TimeEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
        builder.HasIndex(x => x.WorkDate);
    }
}

internal sealed class WorkshopTimeChangeConfiguration : IEntityTypeConfiguration<WorkshopTimeChange>
{
    public void Configure(EntityTypeBuilder<WorkshopTimeChange> builder)
    {
        builder.ToTable("TimeChanges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.EmployeeName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.ChangedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.WorkDate);
    }
}

internal sealed class PayrollMonthConfiguration : IEntityTypeConfiguration<PayrollMonth>
{
    public void Configure(EntityTypeBuilder<PayrollMonth> builder)
    {
        builder.ToTable("PayrollMonths");
        builder.HasKey(x => x.Month);
        builder.Property(x => x.Snapshot).HasColumnType("jsonb");
        builder.Property(x => x.ReportSnapshot).HasColumnType("jsonb");
        builder.Property(x => x.ClosedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.IsClosed);
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmbroideryEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PayrollRoleConfiguration : IEntityTypeConfiguration<PayrollRole>
{
    public void Configure(EntityTypeBuilder<PayrollRole> builder)
    {
        builder.ToTable("PayrollRoles");
        builder.HasKey(x => x.Role);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PayrollPaymentConfiguration : IEntityTypeConfiguration<PayrollPayment>
{
    public void Configure(EntityTypeBuilder<PayrollPayment> builder)
    {
        builder.ToTable("PayrollPayments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.EmployeeName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.Comment).HasMaxLength(PayrollPayment.CommentMaxLength);
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.VoidedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.VoidReason).HasMaxLength(PayrollPayment.ReasonMaxLength);
        builder.Ignore(x => x.IsVoided);
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Month);
    }
}

internal sealed class PayrollAdjustmentConfiguration : IEntityTypeConfiguration<PayrollAdjustment>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustment> builder)
    {
        builder.ToTable("PayrollAdjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.EmployeeName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.Reason).HasMaxLength(PayrollPayment.ReasonMaxLength).IsRequired();
        builder.Property(x => x.CreatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.Property(x => x.VoidedByName).HasMaxLength(ProductionOrder.PersonMaxLength);
        builder.Property(x => x.VoidReason).HasMaxLength(PayrollPayment.ReasonMaxLength);
        builder.Ignore(x => x.IsVoided);
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.Month);
    }
}

internal sealed class PayrollIssuedConfiguration : IEntityTypeConfiguration<PayrollIssued>
{
    public void Configure(EntityTypeBuilder<PayrollIssued> builder)
    {
        builder.ToTable("PayrollIssued");
        builder.HasKey(x => new { x.Month, x.EmployeeId });
        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.UpdatedByName).HasMaxLength(ProductionOrder.PersonMaxLength).IsRequired();
        builder.HasOne<WorkshopEmployee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
