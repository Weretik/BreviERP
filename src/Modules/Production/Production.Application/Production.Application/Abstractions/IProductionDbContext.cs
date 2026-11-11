using Production.Domain.Archive;
using Production.Domain.FabricNorms;
using Production.Domain.Forecast;
using Production.Domain.Orders;
using Production.Domain.Payroll;
using Production.Domain.Settings;
using Production.Domain.Staff;
using Production.Domain.Timesheet;
using Production.Domain.Work;

namespace Production.Application.Abstractions;

/// <summary>Persistence port of the Production module («Швейний цех»); implemented by ProductionDbContext.</summary>
public interface IProductionDbContext
{
    DbSet<WorkshopEmployee> Employees { get; }
    DbSet<ModelFabricNorm> FabricNorms { get; }
    DbSet<ProductionOrder> Orders { get; }
    DbSet<ProductionOrderChange> OrderChanges { get; }
    DbSet<ProductionOrderFile> OrderFiles { get; }
    DbSet<ProductionTelegramDelivery> TelegramDeliveries { get; }
    DbSet<WorkshopSettings> Settings { get; }
    DbSet<ProductionWork> Work { get; }
    DbSet<ForecastQueue> ForecastQueues { get; }
    DbSet<WorkshopTimeEntry> TimeEntries { get; }
    DbSet<WorkshopTimeChange> TimeChanges { get; }
    DbSet<PayrollMonth> PayrollMonths { get; }
    DbSet<PayrollRole> PayrollRoles { get; }
    DbSet<PayrollPayment> PayrollPayments { get; }
    DbSet<PayrollAdjustment> PayrollAdjustments { get; }
    DbSet<PayrollIssued> PayrollIssued { get; }
    DbSet<WorkshopArchiveRecord> Archive { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
