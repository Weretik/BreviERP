using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.RepriceQuote;

/// <summary>
/// «Перерахувати за актуальними цінами» (R-11): <paramref name="Apply"/> false only shows what would change.
/// Manual prices stay; rows the Catalog cannot price any more keep their old prices and are listed.
/// </summary>
public sealed record RepriceQuoteCommand(QuoteActor Actor, long Id, int Version, bool Apply)
    : ICommand<Result<QuoteRepriceReport>>;
