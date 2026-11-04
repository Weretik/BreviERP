using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.CreateQuote;

/// <summary>Creates a draft «КП під запит»; the creator becomes its responsible manager (R-01, R-03).</summary>
public sealed record CreateQuoteCommand(QuoteActor Actor, SaveQuoteInput Input) : ICommand<Result<QuoteResponse>>;
