using Crm.Application.Features.Quotes.Commands.ImportQuotes;

namespace Crm.Api.Endpoints.Quotes.ImportCrmQuotes;

/// <param name="DryRun">True returns the report and saves nothing.</param>
/// <param name="Quotes">The file written by the calculator transfer script.</param>
public sealed record ImportQuotesRequest(bool DryRun, IReadOnlyList<ImportQuoteInput> Quotes);
