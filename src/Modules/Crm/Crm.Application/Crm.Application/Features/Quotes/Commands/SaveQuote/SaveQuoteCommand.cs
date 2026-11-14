using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.SaveQuote;

/// <summary>Saves the whole editor content of a КП; never reprices saved rows (R-11, R-12).</summary>
public sealed record SaveQuoteCommand(QuoteActor Actor, long Id, SaveQuoteInput Input) : ICommand<Result<QuoteResponse>>;
