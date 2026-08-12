using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Create.Validators;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id).InclusiveBetween(1, 1_000_000_000);
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request.RuName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request.Type).IsInEnum();
            RuleFor(x => x.Request.DescriptionUk).NotEmpty().MaximumLength(20_000);
            RuleFor(x => x.Request.DescriptionRu).NotEmpty().MaximumLength(20_000);
            RuleFor(x => x.Request.CategoryIds).NotNull();
            RuleFor(x => x.Request.Photos).NotNull();
            RuleFor(x => x.Request).Must(HaveDataForSelectedType)
                .WithMessage("Product must contain data for its selected type only.");
        });
    }

    private static bool HaveDataForSelectedType(CreateProductCommandRequest request)
        => request.Type == ProductType.Sewing
            ? request.Sewing is not null && request.Ppe is null
            : request.Ppe is not null && request.Sewing is null;
}
