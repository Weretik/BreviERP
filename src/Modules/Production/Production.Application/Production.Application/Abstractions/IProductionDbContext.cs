using Production.Domain.FabricNorms;
using Production.Domain.Staff;

namespace Production.Application.Abstractions;

/// <summary>Persistence port of the Production module («Швейний цех»); implemented by ProductionDbContext.</summary>
public interface IProductionDbContext
{
    DbSet<WorkshopEmployee> Employees { get; }
    DbSet<ModelFabricNorm> FabricNorms { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
