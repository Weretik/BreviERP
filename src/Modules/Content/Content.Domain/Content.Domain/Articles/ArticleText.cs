using Content.Domain.ProductPages;

namespace Content.Domain.Articles;

/// <summary>Title, excerpt, SEO and body of an article in one language. Replaced as a whole when the article is saved.</summary>
public sealed class ArticleText
{
    private ArticleText() { }

    internal ArticleText(string language, ArticleTextInput input)
    {
        Language = language;
        Apply(input);
    }

    internal void Apply(ArticleTextInput input)
    {
        Title = ProductPageRules.Clean(input.Title);
        Excerpt = ProductPageRules.Clean(input.Excerpt);
        SeoTitle = ProductPageRules.Clean(input.SeoTitle);
        SeoDescription = ProductPageRules.Clean(input.SeoDescription);
        Body = ArticleBlocks.Clean(input.Body);
    }

    public int ArticleId { get; private set; }

    public string Language { get; private set; } = null!;

    public string? Title { get; private set; }
    public string? Excerpt { get; private set; }
    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }
    public IReadOnlyList<ArticleBlock> Body { get; private set; } = [];

    public bool IsEmpty =>
        Title is null && Excerpt is null && SeoTitle is null && SeoDescription is null && Body.Count == 0;

    public bool HasSeo => SeoTitle is not null && SeoDescription is not null;
}
