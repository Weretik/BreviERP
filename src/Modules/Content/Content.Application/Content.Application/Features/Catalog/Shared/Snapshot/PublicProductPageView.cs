using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Shared.Snapshot;

/// <summary>
/// What the public catalog needs of a product page, copied out of the <see cref="ProductPage"/> aggregate when the
/// snapshot is built: the cache and the public queries never hold the aggregate (CQRS, PLT-005 R-02).
/// </summary>
public sealed record PublicProductPageView(
    int ProductId,
    string Slug,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<PublicProductPageText> Texts)
{
    public PublicProductPageText? TextIn(string language) => Texts.FirstOrDefault(x => x.Language == language);

    public static PublicProductPageView From(ProductPage page) =>
        new(page.ProductId.Value, page.Slug, page.UpdatedAtUtc,
            page.Texts.Select(x => new PublicProductPageText(x.Language, x.Name, x.SeoTitle, x.SeoDescription,
                x.Summary, x.Description, x.Specs)).ToArray());
}

/// <summary>The texts of a product page in one language, as values.</summary>
public sealed record PublicProductPageText(
    string Language,
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact> Summary,
    IReadOnlyList<ProductTextBlock> Description,
    IReadOnlyList<ProductSpecGroup> Specs)
{
    public bool IsEmpty => Name is null && SeoTitle is null && SeoDescription is null && Summary.Count == 0
                           && Description.Count == 0 && Specs.Count == 0;

    public bool HasText => Summary.Count > 0 || Description.Count > 0;
}
