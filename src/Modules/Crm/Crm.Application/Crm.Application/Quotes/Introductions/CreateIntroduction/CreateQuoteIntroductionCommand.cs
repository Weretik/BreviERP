using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Introductions.Contracts;

namespace Crm.Application.Quotes.Introductions.CreateIntroduction;

/// <summary>Creates a «КП для знайомства»; the creator is its author and responsible person (R-01, R-16).</summary>
public sealed record CreateQuoteIntroductionCommand(QuoteActor Actor, SaveQuoteIntroductionInput Input)
    : ICommand<Result<QuoteResponse>>;
