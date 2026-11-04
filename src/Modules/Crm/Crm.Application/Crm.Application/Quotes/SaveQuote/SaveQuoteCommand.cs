using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.SaveQuote;

/// <summary>Saves the whole editor content of a КП; never reprices saved rows (R-11, R-12).</summary>
public sealed record SaveQuoteCommand(QuoteActor Actor, long Id, SaveQuoteInput Input) : ICommand<Result<QuoteResponse>>;
