using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Introductions.Contracts;

namespace Crm.Application.Quotes.Introductions.SaveIntroduction;

/// <summary>Saves the whole content of a «КП для знайомства»; saved models keep their prices (R-11, R-12).</summary>
public sealed record SaveQuoteIntroductionCommand(QuoteActor Actor, long Id, SaveQuoteIntroductionInput Input)
    : ICommand<Result<QuoteResponse>>;
