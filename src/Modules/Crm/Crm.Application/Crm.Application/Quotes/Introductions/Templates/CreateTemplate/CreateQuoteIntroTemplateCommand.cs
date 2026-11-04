using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Introductions.Contracts;

namespace Crm.Application.Quotes.Introductions.Templates.CreateTemplate;

/// <summary>«Зберегти як готовий шаблон» (R-19).</summary>
public sealed record CreateQuoteIntroTemplateCommand(QuoteActor Actor, CreateQuoteIntroTemplateInput Input)
    : ICommand<Result<QuoteIntroTemplateResponse>>;
