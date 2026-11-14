using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;

/// <summary>Creates a «КП для знайомства»; the creator is its author and responsible person (R-01, R-16).</summary>
public sealed record CreateQuoteIntroductionCommand(QuoteActor Actor, SaveQuoteIntroductionInput Input)
    : ICommand<Result<QuoteResponse>>;
