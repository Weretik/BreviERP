using Catalog.Application.Features.Product.Shared;
using Catalog.Application.Features.Product.Shared.Detail;

namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record GetAdminProductDetailQuery(int Id) : IQuery<Result<ProductAdminDetail>>;
