using Production.Application.Features.FabricNorms.Shared;
namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;

/// <param name="Norm">The workshop norm of the model; null when there is none.</param>
public sealed record ProductionQuoteDraftItem(
    long QuoteItemId, string Name, string? ModelCode, FabricNormInput? Norm,
    IReadOnlyList<ProductionQuoteDraftOption> Options);
