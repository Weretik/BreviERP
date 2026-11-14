using Production.Application.Features.FabricNorms.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Queries.GetProductionQuoteDraft.Results;

namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;

/// <summary>The КП for the order form: its sewing items with fabric variants, norms and, for the workshop, rates.</summary>
public sealed record GetProductionQuoteDraftQuery(ProductionActor Actor, long QuoteId)
    : IQuery<Result<ProductionQuoteDraftResponse>>;
