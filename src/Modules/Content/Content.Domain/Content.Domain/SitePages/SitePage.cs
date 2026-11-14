using BuildingBlocks.Domain.Results;
using Content.Domain.ProductPages;

namespace Content.Domain.SitePages;

/// <summary>
/// Texts of one fixed page of the public site in Ukrainian and Russian: the title of the browser tab and of the search
/// result and the description under it (CNT-007), the main text at the bottom of the page and its frequent questions
/// (CNT-010). An empty text leaves the site's own text.
/// </summary>
public sealed class SitePage
{
    public const int KeyMaxLength = 40;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 320;
    public const int MaxBlocks = 200;
    public const int TextMaxLength = 20_000;
    public const int MaxFaqItems = 12;
    public const int QuestionMaxLength = 200;
    public const int AnswerMaxLength = 1_500;

    private SitePage() { }

    public SitePage(string key, string updatedBy, DateTimeOffset now)
    {
        Key = key;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public string Key { get; private set; } = null!;

    public string? TitleUk { get; private set; }

    public string? DescriptionUk { get; private set; }

    public string? TitleRu { get; private set; }

    public string? DescriptionRu { get; private set; }

    /// <summary>Main text of the page in Ukrainian; empty keeps the site's own.</summary>
    public IReadOnlyList<ProductTextBlock> TextUk { get; private set; } = [];

    public IReadOnlyList<ProductTextBlock> TextRu { get; private set; } = [];

    /// <summary>Frequent questions in Ukrainian; empty keeps the site's own.</summary>
    public IReadOnlyList<SiteFaqItem> FaqUk { get; private set; } = [];

    public IReadOnlyList<SiteFaqItem> FaqRu { get; private set; } = [];

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public DomainResult Replace(SitePageTextInput uk, SitePageTextInput ru, string updatedBy, DateTimeOffset now)
    {
        var (titleUk, descriptionUk, titleRu, descriptionRu) = (uk.Title, uk.Description, ru.Title, ru.Description);
        var (textUk, textRu) = (ProductTextBlocks.Clean(uk.Text), ProductTextBlocks.Clean(ru.Text));
        var (faqUk, faqRu) = (SiteFaqItem.Clean(uk.Faq), SiteFaqItem.Clean(ru.Faq));
        if (textUk.Count > MaxBlocks || textRu.Count > MaxBlocks || TextLength(textUk) > TextMaxLength
            || TextLength(textRu) > TextMaxLength)
            return SitePageErrors.TextTooLong();
        if (faqUk.Count > MaxFaqItems || faqRu.Count > MaxFaqItems
            || faqUk.Concat(faqRu).Any(x => x.Question.Length > QuestionMaxLength || x.Answer.Length > AnswerMaxLength))
            return SitePageErrors.FaqTooLong();
        (titleUk, descriptionUk, titleRu, descriptionRu) =
            (Clean(titleUk), Clean(descriptionUk), Clean(titleRu), Clean(descriptionRu));
        if (titleUk?.Length > TitleMaxLength || titleRu?.Length > TitleMaxLength) return SitePageErrors.TitleTooLong();
        if (descriptionUk?.Length > DescriptionMaxLength || descriptionRu?.Length > DescriptionMaxLength)
            return SitePageErrors.DescriptionTooLong();
        TextUk = textUk;
        TextRu = textRu;
        FaqUk = faqUk;
        FaqRu = faqRu;
        TitleUk = titleUk;
        DescriptionUk = descriptionUk;
        TitleRu = titleRu;
        DescriptionRu = descriptionRu;
        Revision++;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
        return DomainResult.Success();
    }

    /// <summary>Characters of a main text, the way staff count them in the editor.</summary>
    public static int TextLength(IEnumerable<ProductTextBlock?>? blocks) =>
        (blocks ?? []).Where(x => x is not null).Sum(x =>
            (x!.Text?.Length ?? 0) + (x.Lead?.Length ?? 0) + (x.Items?.Sum(item => item?.Length ?? 0) ?? 0));

    /// <summary>Trims the text and joins lines: a title or description is one line.</summary>
    public static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
