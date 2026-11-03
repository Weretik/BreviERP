using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Overview.Shared.Specifications;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.CodeCheck;

public sealed class GetProductCodeCheckQueryHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetProductCodeCheckQuery, Result<ProductCodeCheck>>
{
    public async ValueTask<Result<ProductCodeCheck>> Handle(GetProductCodeCheckQuery query, CancellationToken cancellationToken)
    {
        if (!ProductOverviewTypes.TryParse(query.Type, out var type))
            return Result.Invalid(new ValidationError(nameof(query.Type), "Тип товару: Sewing або Ppe."));
        var code = string.IsNullOrWhiteSpace(query.Code) ? null : query.Code.Trim();
        if (code?.Length > ProductEntity.CodeMaxLength)
            return Result.Invalid(new ValidationError(nameof(query.Code), "Код — до 80 символів."));

        var suggested = type == ProductType.Sewing
            ? SewingModelCodes.Next(await repository.ListAsync(new SewingModelCodesSpec(), cancellationToken))
            : null;
        if (code is null) return Result.Success(new ProductCodeCheck(null, null, null, null, suggested));

        var taken = await repository.FirstOrDefaultAsync(new ProductByCodeSpec(type, code), cancellationToken);
        return Result.Success(new ProductCodeCheck(code, taken is null, taken?.Id.Value, taken?.Name, suggested));
    }
}
