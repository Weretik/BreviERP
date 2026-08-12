using Catalog.Application.Contracts.Admin;

namespace Catalog.Application.Features.Product.GetAdminDetail;

public sealed record GetAdminProductDetailQuery(int Id) : IQuery<Result<ProductAdminDetail>>;
