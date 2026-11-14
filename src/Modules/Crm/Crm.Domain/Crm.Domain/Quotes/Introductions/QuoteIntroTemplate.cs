using BuildingBlocks.Domain.Results;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.Errors;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Domain.Quotes.Introductions;

/// <summary>
/// «Готовий шаблон» of a «КП для знайомства» (R-19): the layout, the models with their fabrics and the letter variant.
/// It keeps no prices; applying it reads the current Catalog. All staff see every template.
/// </summary>
public sealed class QuoteIntroTemplate
{
    private QuoteIntroTemplate() { }

    public static DomainResult<QuoteIntroTemplate> Create(string name, QuoteIntroLayout layout, string? letterName,
        IReadOnlyList<QuoteIntroTemplatePage> pages, QuotePerson author, DateTimeOffset now) =>
        string.IsNullOrWhiteSpace(name)
            ? QuoteErrors.TemplateNameRequired()
            : new QuoteIntroTemplate(name, layout, letterName, pages, author, now);

    private QuoteIntroTemplate(string name, QuoteIntroLayout layout, string? letterName,
        IReadOnlyList<QuoteIntroTemplatePage> pages, QuotePerson author, DateTimeOffset now)
    {
        Name = name.Trim();
        NameKey = Name.ToLowerInvariant();
        Layout = layout;
        LetterName = QuoteText.Normalize(letterName);
        Pages = pages;
        AuthorUserId = author.UserId;
        AuthorName = author.Name;
        CreatedAt = now;
    }

    public long Id { get; private set; }
    public string Name { get; private set; } = null!;

    /// <summary>The name in lower case; unique, so two templates never differ only by letter case.</summary>
    public string NameKey { get; private set; } = null!;

    public QuoteIntroLayout Layout { get; private set; }
    public string? LetterName { get; private set; }
    public IReadOnlyList<QuoteIntroTemplatePage> Pages { get; private set; } = [];
    public int AuthorUserId { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}

/// <summary>One page: the main model first, then the three chosen ones.</summary>
public sealed record QuoteIntroTemplatePage(QuoteIntroSeason Season, IReadOnlyList<QuoteIntroTemplateModel> Models);

/// <summary>A model and its fabric; the names are kept for the template list only.</summary>
public sealed record QuoteIntroTemplateModel(int ProductId, long ArticleId, string ProductName, string FabricTitle);

public static class QuoteIntroTemplateLimits
{
    public const int NameMaxLength = 100;
    public const int MaxTemplates = 200;
}
