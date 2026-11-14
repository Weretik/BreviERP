using Catalog.Application.Features.Product.Shared.Photos;

namespace Catalog.Application.Features.Product.Queries.GetProductSkuIndex;

/// <summary>Every product with its sale SKUs, for imports that match old-site articles (CAT-016).</summary>
public sealed record GetProductSkuIndexQuery : IQuery<Result<IReadOnlyList<ProductSkuIndexItem>>>;
