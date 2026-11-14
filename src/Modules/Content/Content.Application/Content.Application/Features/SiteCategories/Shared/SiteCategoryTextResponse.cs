using Content.Domain.ProductPages;

namespace Content.Application.Features.SiteCategories.Shared;

/// <summary>Texts of one language; an empty list and nulls when nothing is written.</summary>
public sealed record SiteCategoryTextResponse(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock> Text);
