using BuildingBlocks.Application.Caching;
using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Commands.DeleteContentProduct;

/// <summary>Removes the site page of a product with its texts (CNT-002 R-06); the Catalog product stays.</summary>
public sealed record DeleteContentProductCommand(int ProductId) : ICommand<Result>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [PublicSiteCache.Tag];
}
