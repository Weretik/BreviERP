using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Products;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;

/// <summary>A product page of the site by its address. Unknown, hidden and inactive products are all «not found».</summary>
public sealed record GetPublicCatalogProductQuery(string Slug, string Language) : IQuery<Result<PublicCatalogProduct>>;
