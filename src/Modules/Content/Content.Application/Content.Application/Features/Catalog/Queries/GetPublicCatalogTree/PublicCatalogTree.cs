using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;

/// <summary>The catalog tree of the site: the two product types and, under each, the sections that have products on the site.</summary>
public sealed record PublicCatalogTree(IReadOnlyList<PublicCatalogType> Types);
