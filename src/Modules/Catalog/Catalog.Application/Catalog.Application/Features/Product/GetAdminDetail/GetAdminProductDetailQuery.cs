using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Admin.Product;

namespace Catalog.Application.Features.Product.GetAdminDetail;

public sealed record GetAdminProductDetailQuery(int Id) : IQuery<Result<ProductAdminDetail>>;
