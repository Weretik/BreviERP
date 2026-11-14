using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Commands.CreateQuote;

/// <summary>Creates a draft «КП під запит»; the creator becomes its responsible manager (R-01, R-03).</summary>
public sealed record CreateQuoteCommand(QuoteActor Actor, SaveQuoteInput Input) : ICommand<Result<QuoteResponse>>;
