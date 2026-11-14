using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;

/// <summary>The catalog tree of the site: types and their sections with products on the site (CNT-002 R-09).</summary>
public sealed record GetPublicCatalogTreeQuery : IQuery<Result<PublicCatalogTree>>;
