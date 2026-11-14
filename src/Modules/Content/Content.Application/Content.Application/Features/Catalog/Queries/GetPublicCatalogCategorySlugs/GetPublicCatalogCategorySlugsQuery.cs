using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Links;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;

/// <summary>Addresses of every shown category, for the sitemap and for building the pages ahead of time.</summary>
public sealed record GetPublicCatalogCategorySlugsQuery : IQuery<Result<IReadOnlyList<PublicCatalogSlug>>>;
