using BuildingBlocks.Domain.Results;
using Content.Domain.ProductPages;

namespace Content.Domain.Articles;

/// <summary>
/// An article of the public site (CNT-004): its address, the date shown to readers, the cover photo of the «Фото»
/// library and the texts by language. The site shows it only while <see cref="ShowOnSite"/> is on.
/// </summary>
public sealed class Article
{
    private readonly List<ArticleText> _texts = [];

    private Article() { }

    private Article(string slug, DateOnly publishedOn, string updatedBy, DateTimeOffset now)
    {
        Slug = slug;
        PublishedOn = publishedOn;
        Touch(updatedBy, now, bump: false);
    }

    public static DomainResult<Article> Create(string slug, DateOnly publishedOn, string updatedBy, DateTimeOffset now)
    {
        if (!ProductPageRules.IsValidSlug(slug)) return ProductPageErrors.InvalidSlug();
        return new Article(slug, publishedOn, updatedBy, now);
    }

    public int Id { get; private set; }

    public bool ShowOnSite { get; private set; }

    public string Slug { get; private set; } = null!;

    /// <summary>The date readers see; newer articles come first.</summary>
    public DateOnly PublishedOn { get; private set; }

    /// <summary>The cover in the «Фото» library; the id of another module, no FK.</summary>
    public int? CoverPhotoId { get; private set; }

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public IReadOnlyList<ArticleText> Texts => _texts;

    public ArticleText? TextIn(string language) => _texts.FirstOrDefault(x => x.Language == language);

    /// <summary>The cover and every photo of the bodies, each once.</summary>
    public IReadOnlyList<int> PhotoIds() =>
        (CoverPhotoId is { } cover ? new[] { cover } : [])
        .Concat(ArticleBlocks.PhotoIds(_texts.SelectMany(x => x.Body))).Distinct().ToArray();

    public void SetVisibility(bool showOnSite, string updatedBy, DateTimeOffset now)
    {
        if (ShowOnSite == showOnSite) return;
        ShowOnSite = showOnSite;
        Touch(updatedBy, now, bump: true);
    }

    /// <summary>Replaces the article as a whole; a language without any text loses its row.</summary>
    public DomainResult Replace(string slug, DateOnly publishedOn, bool showOnSite, int? coverPhotoId,
        IReadOnlyDictionary<string, ArticleTextInput> texts, string updatedBy, DateTimeOffset now)
    {
        if (!ProductPageRules.IsValidSlug(slug)) return ProductPageErrors.InvalidSlug();
        Slug = slug;
        PublishedOn = publishedOn;
        ShowOnSite = showOnSite;
        CoverPhotoId = coverPhotoId;
        foreach (var language in ProductPageRules.Languages)
        {
            var text = TextIn(language);
            if (!texts.TryGetValue(language, out var input))
            {
                if (text is not null) _texts.Remove(text);
                continue;
            }

            if (text is null) _texts.Add(text = new ArticleText(language, input));
            else text.Apply(input);
            if (text.IsEmpty) _texts.Remove(text);
        }

        Touch(updatedBy, now, bump: true);
        return DomainResult.Success();
    }

    private void Touch(string updatedBy, DateTimeOffset now, bool bump)
    {
        UpdatedBy = updatedBy.Length > ProductPageRules.UpdatedByMaxLength
            ? updatedBy[..ProductPageRules.UpdatedByMaxLength]
            : updatedBy;
        UpdatedAtUtc = now.ToUniversalTime();
        if (bump) Revision++;
    }
}
