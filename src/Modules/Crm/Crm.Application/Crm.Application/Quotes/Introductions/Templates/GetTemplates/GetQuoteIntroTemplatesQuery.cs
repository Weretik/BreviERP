using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Introductions.Contracts;

namespace Crm.Application.Quotes.Introductions.Templates.GetTemplates;

/// <summary>«Готові шаблони»: all of them, newest first (R-19).</summary>
public sealed record GetQuoteIntroTemplatesQuery(QuoteActor Actor) : IQuery<Result<IReadOnlyList<QuoteIntroTemplateResponse>>>;
