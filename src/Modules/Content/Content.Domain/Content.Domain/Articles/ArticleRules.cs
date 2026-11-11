namespace Content.Domain.Articles;

/// <summary>Limits of a site article (CNT-004 R-01). The slug, languages and SEO limits are those of product pages.</summary>
public static class ArticleRules
{
    public const int TitleMaxLength = 200;
    public const int ExcerptMaxLength = 500;
    public const int AltMaxLength = 200;
    public const int MaxBlocks = 300;
    public const int TextMaxLength = 60_000;
    public const int MaxArticles = 2_000;
}
