using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.History;

/// <summary>«Історія змін» of a КП, newest first (R-22).</summary>
public sealed record GetQuoteHistoryQuery(QuoteActor Actor, long Id) : IQuery<Result<IReadOnlyList<QuoteEventResponse>>>;
