using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteHistory;

/// <summary>«Історія змін» of a КП, newest first (R-22).</summary>
public sealed record GetQuoteHistoryQuery(QuoteActor Actor, long Id) : IQuery<Result<IReadOnlyList<QuoteEventResponse>>>;
