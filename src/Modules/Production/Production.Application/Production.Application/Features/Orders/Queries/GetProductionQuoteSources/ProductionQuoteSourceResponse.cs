using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Queries.GetProductionQuoteSources;

/// <summary>A КП «під запит» the person may pass to the workshop.</summary>
/// <param name="OrdersCount">Orders already made from it.</param>
public sealed record ProductionQuoteSourceResponse(
    long Id, string Number, string ClientName, string ResponsibleName, DateTimeOffset? LastSentAt, int OrdersCount);
