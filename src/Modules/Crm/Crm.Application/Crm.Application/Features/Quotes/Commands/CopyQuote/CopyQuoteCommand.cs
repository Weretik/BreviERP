using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.CopyQuote;

/// <summary>
/// «Копіювати КП» (R-24): a new draft with the next number and the same header, introduction and items with their
/// saved prices; the person who copies becomes its author and responsible. An archived КП can be copied too.
/// </summary>
public sealed record CopyQuoteCommand(QuoteActor Actor, long Id) : ICommand<Result<QuoteResponse>>;
