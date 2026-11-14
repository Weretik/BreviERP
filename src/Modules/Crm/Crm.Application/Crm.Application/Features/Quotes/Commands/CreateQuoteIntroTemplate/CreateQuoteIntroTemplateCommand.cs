using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroTemplate;

/// <summary>«Зберегти як готовий шаблон» (R-19).</summary>
public sealed record CreateQuoteIntroTemplateCommand(QuoteActor Actor, CreateQuoteIntroTemplateInput Input)
    : ICommand<Result<QuoteIntroTemplateResponse>>;
