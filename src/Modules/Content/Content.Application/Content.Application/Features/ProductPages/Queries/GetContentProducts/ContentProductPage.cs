using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.ProductPages.Queries.GetContentProducts;

public sealed record ContentProductPage(IReadOnlyList<ContentProductRow> Items, int Page, int PageSize, int Total);
