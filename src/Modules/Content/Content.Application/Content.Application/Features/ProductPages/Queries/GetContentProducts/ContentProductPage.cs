using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Queries.GetContentProducts;

public sealed record ContentProductPage(IReadOnlyList<ContentProductRow> Items, int Page, int PageSize, int Total);
