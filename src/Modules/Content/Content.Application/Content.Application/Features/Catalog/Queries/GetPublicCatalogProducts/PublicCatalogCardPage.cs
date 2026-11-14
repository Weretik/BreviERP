using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Products;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

public sealed record PublicCatalogCardPage(IReadOnlyList<PublicCatalogCard> Items, int Page, int PageSize, int Total);
