using Catalog.Application.Features.Product.Overview.Shared;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.CodeCheck;

public sealed class GetProductCodeCheckQueryValidator : AbstractValidator<GetProductCodeCheckQuery>
{
    public GetProductCodeCheckQueryValidator()
    {
        RuleFor(x => x.Type).Must(type => ProductOverviewTypes.TryParse(type, out _))
            .WithMessage("Тип товару: Sewing або Ppe.");
        RuleFor(x => x.Code).Must(code => code is null || code.Trim().Length <= ProductEntity.CodeMaxLength)
            .WithMessage($"Код — до {ProductEntity.CodeMaxLength} символів.");
    }
}
