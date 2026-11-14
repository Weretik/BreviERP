using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Links;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;

/// <summary>Addresses of every product on the site, for the sitemap and for building the pages ahead of time.</summary>
public sealed record GetPublicCatalogProductSlugsQuery : IQuery<Result<IReadOnlyList<PublicCatalogSlug>>>;
