using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft.Results;

/// <summary>A «Вишивка» row of the КП whose saved calculation still exists.</summary>
public sealed record ProductionQuoteDraftEmbroidery(long QuoteItemId, ProductionEmbroideryChoiceResponse Calculation);
