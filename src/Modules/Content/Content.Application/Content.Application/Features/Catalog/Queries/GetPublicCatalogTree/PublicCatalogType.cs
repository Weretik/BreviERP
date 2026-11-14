using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;

/// <param name="Type"><c>Sewing</c> or <c>Ppe</c>.</param>
/// <param name="ProductCount">Products of the type on the site, with and without a section.</param>
public sealed record PublicCatalogType(string Type, int ProductCount, IReadOnlyList<PublicCatalogSection> Sections);
