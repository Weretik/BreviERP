using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

/// <summary>«Розрахунок вишивки» in the order form: saved calculations to pick, newest first.</summary>
public sealed record GetProductionEmbroideriesQuery(ProductionActor Actor, string? Search)
    : IQuery<Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>>;
