using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

/// <summary>What staff type for one language of a site category: the body of the save request and of the old-site import.</summary>
public sealed record SiteCategoryTextInput(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock>? Text)
{
    public SiteCategoryTextValues ToValues() => new(Name, SeoTitle, SeoDescription, Text);
}
