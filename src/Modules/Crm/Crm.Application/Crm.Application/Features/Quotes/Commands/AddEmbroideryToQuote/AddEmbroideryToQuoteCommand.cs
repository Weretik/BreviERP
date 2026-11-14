using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Embroidery;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;

namespace Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;

/// <summary>
/// «Додати в КП» (EMB-001 R-12): appends a saved embroidery calculation as one item at the end of a КП. It needs no
/// version: it adds and never overwrites, and an editor open elsewhere sees the change as a stale save.
/// </summary>
/// <remarks>The client sees the one price category saved with the calculation.</remarks>
public sealed record AddEmbroideryToQuoteCommand(QuoteActor Actor, long QuoteId, long CalculationId)
    : ICommand<Result<QuoteResponse>>;
