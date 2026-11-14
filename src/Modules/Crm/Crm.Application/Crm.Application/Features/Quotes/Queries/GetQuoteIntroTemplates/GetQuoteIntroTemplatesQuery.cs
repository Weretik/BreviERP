using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteIntroTemplates;

/// <summary>«Готові шаблони»: all of them, newest first (R-19).</summary>
public sealed record GetQuoteIntroTemplatesQuery(QuoteActor Actor) : IQuery<Result<IReadOnlyList<QuoteIntroTemplateResponse>>>;
