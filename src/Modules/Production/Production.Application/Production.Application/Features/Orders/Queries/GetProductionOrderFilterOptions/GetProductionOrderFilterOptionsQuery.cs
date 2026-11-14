using Production.Application.Abstractions;
using Production.Domain.FabricPlans;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;

/// <summary>What «Фільтри» of «Виробничі завдання» offer: values of the orders that are not deleted.</summary>
public sealed record GetProductionOrderFilterOptionsQuery : IQuery<Result<ProductionOrderFilterOptionsResponse>>;
