using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;

/// <summary>
/// «Витрата за період»: received, issued (to tasks and one-off) and corrected metres of each line over a period, by
/// operation date. A reversal counts against the kind it takes back; taking back an opening counts as a correction.
/// </summary>
public sealed record GetFabricConsumptionQuery(WarehouseActor Actor, DateOnly From, DateOnly To)
    : IQuery<Result<FabricConsumptionResponse>>;
