using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;

/// <param name="Slug">Built from the Ukrainian name; the same for both languages.</param>
public sealed record PublicCatalogSection(int Id, string Slug, string Name, int ProductCount);
