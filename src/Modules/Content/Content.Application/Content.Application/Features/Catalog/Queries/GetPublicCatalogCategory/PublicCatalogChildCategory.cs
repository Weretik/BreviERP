using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;

public sealed record PublicCatalogChildCategory(string Slug, string Name, int ProductCount);
