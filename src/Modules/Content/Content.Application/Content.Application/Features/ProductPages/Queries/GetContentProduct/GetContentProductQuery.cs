using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.ProductPages.Queries.GetContentProduct;

/// <summary>The editor of one product; a product without a page comes with empty texts and an offered slug.</summary>
public sealed record GetContentProductQuery(int ProductId) : IQuery<Result<ContentProductResponse>>;
