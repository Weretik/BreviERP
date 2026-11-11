namespace Content.Domain.SiteCategories;

/// <summary>Limits of a site category (CNT-003 R-01). The slug, languages and SEO limits are those of product pages.</summary>
public static class SiteCategoryRules
{
    public const int NameMaxLength = 200;
    public const int MaxBlocks = 200;
    public const int TextMaxLength = 20_000;
    public const int MaxCategories = 500;
}
