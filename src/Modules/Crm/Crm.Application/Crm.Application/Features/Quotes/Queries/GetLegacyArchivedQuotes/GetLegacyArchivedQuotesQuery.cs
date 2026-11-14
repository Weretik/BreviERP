using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetLegacyArchivedQuotes;

/// <summary>How many КП are left from the removed «Архів» (R-23); shown to the Director before the purge.</summary>
public sealed record GetLegacyArchivedQuotesQuery(QuoteActor Actor) : IQuery<Result<LegacyArchivedQuotesResponse>>;
