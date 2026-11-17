using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Commands.SaveContentProduct;

/// <summary>What staff type for one language of a product page: the body of the save request and of the old-site import.</summary>
public sealed record ProductPageTextInput(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact>? Summary,
    IReadOnlyList<ProductTextBlock>? Description,
    IReadOnlyList<ProductSpecGroup>? Specs)
{
    public ProductPageTextValues ToValues() => new(Name, SeoTitle, SeoDescription, Summary, Description, Specs);
}
