using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.LegacyArchive;

/// <summary>Deletes every КП left from the removed «Архів»; one with a production order returns to the list.</summary>
public sealed record PurgeLegacyArchivedQuotesCommand(QuoteActor Actor) : ICommand<Result<LegacyArchivePurgeResponse>>;
