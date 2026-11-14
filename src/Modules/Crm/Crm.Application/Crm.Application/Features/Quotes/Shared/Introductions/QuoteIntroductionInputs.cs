namespace Crm.Application.Features.Quotes.Shared.Introductions;

/// <summary>
/// The whole editable content of a «КП для знайомства» (R-16, R-17). Prices are never taken from the client: a new
/// model gets the published від 41 шт. price from the Catalog, a saved one keeps its own.
/// </summary>
/// <param name="Version">The version the editor loaded; ignored on create.</param>
/// <param name="Layout">`General`, `Summer` or `Winter`.</param>
/// <param name="LetterName">A letter variant of «Налаштування КП».</param>
/// <param name="Pages">One page per season of the layout, summer first.</param>
/// <param name="LetterSubject">The variant's subject changed for this КП; null keeps the variant's.</param>
/// <param name="LetterBody">The variant's text changed for this КП; null keeps the variant's.</param>
public sealed record SaveQuoteIntroductionInput(
    int Version,
    string InternalName,
    long? CustomerId,
    string? RecipientEmail,
    string? RecipientName,
    string? CompanyName,
    string? ManagerNote,
    string Layout,
    string? LetterName,
    IReadOnlyList<QuoteIntroPageInput> Pages,
    string? LetterSubject = null,
    string? LetterBody = null);

/// <param name="Season">`Summer` or `Winter`.</param>
/// <param name="Models">The main model of the season first, then three more.</param>
public sealed record QuoteIntroPageInput(string Season, IReadOnlyList<QuoteIntroModelInput> Models);

/// <param name="ItemId">The saved item, so its price stays; null for a new model.</param>
public sealed record QuoteIntroModelInput(long? ItemId, int ProductId, long ArticleId);

/// <summary>«Зберегти як готовий шаблон» (R-19).</summary>
public sealed record CreateQuoteIntroTemplateInput(
    string Name,
    string Layout,
    string? LetterName,
    IReadOnlyList<QuoteIntroPageInput> Pages);
