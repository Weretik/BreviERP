using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.Product.Shared.Overview;

namespace Catalog.Application.Features.Product.Queries.GetProductCodeCheck;

public sealed class GetProductCodeCheckHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetProductCodeCheckQuery, Result<ProductCodeCheck>>
{
    public async ValueTask<Result<ProductCodeCheck>> Handle(GetProductCodeCheckQuery query, CancellationToken cancellationToken)
    {
        ProductOverviewTypes.TryParse(query.Type, out var type);
        var code = string.IsNullOrWhiteSpace(query.Code) ? null : query.Code.Trim();

        var suggested = type == ProductType.Sewing
            ? SewingModelCodes.Next(await repository.ListAsync(new SewingModelCodesSpec(), cancellationToken))
            : null;
        if (code is null) return Result.Success(new ProductCodeCheck(null, null, null, null, suggested));

        var taken = await repository.FirstOrDefaultAsync(new ProductByCodeSpec(type, code), cancellationToken);
        return Result.Success(new ProductCodeCheck(code, taken is null, taken?.Id.Value, taken?.Name, suggested));
    }
}
