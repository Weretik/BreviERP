using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft.Results;

/// <summary>The КП as the order form needs it: its sewing items with every fabric variant.</summary>
/// <param name="Embroideries">«Вишивка» rows of the КП; the form puts them into the items' applications.</param>
public sealed record ProductionQuoteDraftResponse(
    long QuoteId, string Number, string ClientName, string ManagerName, IReadOnlyList<ProductionQuoteDraftItem> Items,
    IReadOnlyList<ProductionQuoteDraftEmbroidery> Embroideries);
