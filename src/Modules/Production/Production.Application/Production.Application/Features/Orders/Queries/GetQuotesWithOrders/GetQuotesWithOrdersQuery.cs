namespace Production.Application.Features.Orders.Queries.GetQuotesWithOrders;

/// <summary>Which of these КП have a production order that is not deleted; Crm asks before deleting a КП.</summary>
public sealed record GetQuotesWithOrdersQuery(IReadOnlyCollection<long> QuoteIds) : IQuery<Result<IReadOnlyList<long>>>;
