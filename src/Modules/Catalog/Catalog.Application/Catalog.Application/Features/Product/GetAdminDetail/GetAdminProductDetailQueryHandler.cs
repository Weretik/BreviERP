using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Features.Product.Specifications;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminDetail;

public sealed class GetAdminProductDetailQueryHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetAdminProductDetailQuery, Result<ProductAdminDetail>>
{
    public async ValueTask<Result<ProductAdminDetail>> Handle(GetAdminProductDetailQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdWithDetailsSpec(query.Id), cancellationToken);
        return product is null ? Result.NotFound() : Result.Success(ProductAdminDetailMapper.Map(product));
    }
}
