using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Features.Product.Shared.Overview;

namespace Catalog.Application.Features.Product.Queries.GetProductCodeCheck;

public sealed class GetProductCodeCheckValidator : AbstractValidator<GetProductCodeCheckQuery>
{
    public GetProductCodeCheckValidator()
    {
        RuleFor(x => x.Type).Must(type => ProductOverviewTypes.TryParse(type, out _))
            .WithMessage("Тип товару: Sewing або Ppe.");
        RuleFor(x => x.Code).Must(code => code is null || code.Trim().Length <= ProductEntity.CodeMaxLength)
            .WithMessage($"Код — до {ProductEntity.CodeMaxLength} символів.");
    }
}
