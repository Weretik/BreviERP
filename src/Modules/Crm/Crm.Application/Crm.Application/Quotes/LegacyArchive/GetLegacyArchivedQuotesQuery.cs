using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.LegacyArchive;

/// <summary>How many КП are left from the removed «Архів» (R-23); shown to the Director before the purge.</summary>
public sealed record GetLegacyArchivedQuotesQuery(QuoteActor Actor) : IQuery<Result<LegacyArchivedQuotesResponse>>;
