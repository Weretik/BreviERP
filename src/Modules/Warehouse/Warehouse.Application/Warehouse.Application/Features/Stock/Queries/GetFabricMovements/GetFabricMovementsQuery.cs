using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

/// <summary>
/// «Рух тканини» (WH-007): 50 movements per page, newest operation date first (then record time and id). Optional
/// filters: a kind, a fabric (any code of its group), its colour, an order item, a text search and an operation-date
/// period. Counts per kind follow every filter but the kind.
/// </summary>
public sealed record GetFabricMovementsQuery(
    WarehouseActor Actor,
    int Page,
    FabricMovementFilter Kind,
    string? FabricCode,
    string? Color,
    int? ItemId,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null) : IQuery<Result<FabricMovementPageResponse>>;
