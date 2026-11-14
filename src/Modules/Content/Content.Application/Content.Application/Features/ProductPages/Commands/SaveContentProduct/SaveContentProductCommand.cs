using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.ProductPages.Commands.SaveContentProduct;

/// <summary>
/// Creates the page of a product or replaces it as a whole (CNT-002 R-05). <paramref name="Revision"/> is the number
/// the editor showed: 0 for a product without a page. An empty <paramref name="Slug"/> is built from the name.
/// <paramref name="CategoryIds"/> replaces the site categories of the product (CNT-003 R-03); null keeps them.
/// </summary>
public sealed record SaveContentProductCommand(
    int ProductId,
    int Revision,
    string? Slug,
    bool ShowOnSite,
    ProductPageTextInput? Uk,
    ProductPageTextInput? Ru,
    string UpdatedBy,
    IReadOnlyList<int>? CategoryIds = null) : ICommand<Result<ContentProductResponse>>
{
    public const int MaxCategories = 50;
}
