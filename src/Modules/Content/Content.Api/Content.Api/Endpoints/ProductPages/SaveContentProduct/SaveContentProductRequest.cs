using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

namespace Content.Api.Endpoints.ProductPages.SaveContentProduct;

/// <summary>
/// Body of <c>saveContentProduct</c>; <c>Revision</c> is 0 for a product without a page. <c>CategoryIds</c> replaces the
/// site categories of the product; null keeps them.
/// </summary>
public sealed record SaveContentProductRequest(
    int Revision, string? Slug, bool ShowOnSite, ProductPageTextInput? Uk, ProductPageTextInput? Ru,
    IReadOnlyList<int>? CategoryIds = null);
