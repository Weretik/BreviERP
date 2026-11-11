using Content.Domain.ProductPages;

namespace Content.Domain.SiteCategories;

/// <summary>What staff type for one language of a site category.</summary>
public sealed record SiteCategoryTextInput(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock>? Text);
