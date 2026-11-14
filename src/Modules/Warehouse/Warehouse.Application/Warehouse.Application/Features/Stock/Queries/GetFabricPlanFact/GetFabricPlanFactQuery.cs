using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

/// <summary>
/// «План і факт» (TZ §11): main fabric by model for the items whose fabric was issued for all their products. Filters
/// match a part of the model, the client, or a planned or issued fabric (code, name or colour); the period keeps the
/// items whose last issue is in it (WH-010).
/// </summary>
public sealed record GetFabricPlanFactQuery(WarehouseActor Actor, string? Model, string? Client, string? Fabric,
    DateOnly? From = null, DateOnly? To = null) : IQuery<Result<FabricPlanFactResponse>>;
