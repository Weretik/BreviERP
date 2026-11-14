using Catalog.Application.Features.Product.Shared;

namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record GetAdminProductDetailQuery(int Id) : IQuery<Result<ProductAdminDetail>>;
