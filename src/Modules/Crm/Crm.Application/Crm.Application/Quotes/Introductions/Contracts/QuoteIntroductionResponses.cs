using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.Introductions.Contracts;

/// <summary>What the introduction editor needs and a Manager cannot read from the settings.</summary>
/// <param name="Letters">Letter variants with their subjects and texts, in the settings order.</param>
public sealed record QuoteIntroductionSetupResponse(
    QuoteIntroMainModelResponse Summer,
    QuoteIntroMainModelResponse Winter,
    IReadOnlyList<QuoteIntroLetterResponse> Letters);

/// <param name="Product">Null when no active sewing model has the code.</param>
/// <param name="Problem">Why the page cannot be built; null when it can.</param>
public sealed record QuoteIntroMainModelResponse(string Code, QuoteCatalogProductResponse? Product, string? Problem);

/// <summary>A letter variant as written in the settings, with its {marks} filled only when sending.</summary>
public sealed record QuoteIntroLetterResponse(string Name, string Subject, string Body);

public sealed record QuoteIntroTemplateResponse(
    long Id,
    string Name,
    string Layout,
    string? LetterName,
    IReadOnlyList<QuoteIntroTemplatePageResponse> Pages,
    QuotePersonResponse Author,
    DateTimeOffset CreatedAt,
    bool CanDelete);

public sealed record QuoteIntroTemplatePageResponse(string Season, IReadOnlyList<QuoteIntroTemplateModelResponse> Models);

public sealed record QuoteIntroTemplateModelResponse(int ProductId, long ArticleId, string ProductName, string FabricTitle);
