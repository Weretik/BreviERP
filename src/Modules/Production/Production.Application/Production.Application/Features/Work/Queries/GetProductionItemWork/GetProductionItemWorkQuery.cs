using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetProductionItemWork;

/// <summary>«Робота» of an item in the order card.</summary>
public sealed record GetProductionItemWorkQuery(ProductionActor Actor, int OrderId, int ItemId)
    : IQuery<Result<ProductionItemWorkResponse>>;
