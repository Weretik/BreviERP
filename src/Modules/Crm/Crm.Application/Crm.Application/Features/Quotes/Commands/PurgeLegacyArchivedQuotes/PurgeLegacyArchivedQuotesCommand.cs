using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;

/// <summary>Deletes every КП left from the removed «Архів»; one with a production order returns to the list.</summary>
public sealed record PurgeLegacyArchivedQuotesCommand(QuoteActor Actor) : ICommand<Result<LegacyArchivePurgeResponse>>;
