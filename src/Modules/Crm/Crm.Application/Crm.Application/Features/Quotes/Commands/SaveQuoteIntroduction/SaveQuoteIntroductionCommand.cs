using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;

/// <summary>Saves the whole content of a «КП для знайомства»; saved models keep their prices (R-11, R-12).</summary>
public sealed record SaveQuoteIntroductionCommand(QuoteActor Actor, long Id, SaveQuoteIntroductionInput Input)
    : ICommand<Result<QuoteResponse>>;
