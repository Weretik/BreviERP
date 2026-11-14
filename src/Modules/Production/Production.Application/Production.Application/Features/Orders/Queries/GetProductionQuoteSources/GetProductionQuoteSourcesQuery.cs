using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Queries.GetProductionQuoteSources;

/// <summary>КП «під запит» the person may pass to the workshop, with how many orders each already has.</summary>
public sealed record GetProductionQuoteSourcesQuery(ProductionActor Actor, string? Search)
    : IQuery<Result<IReadOnlyList<ProductionQuoteSourceResponse>>>;
