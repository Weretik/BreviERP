using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;

public sealed class UpdatePpeSupplierOfferValidator : AbstractValidator<UpdatePpeSupplierOfferCommand>
{
    public UpdatePpeSupplierOfferValidator()
    {
        RuleFor(x => x.ProductId).InclusiveBetween(1, 1_000_000_000);
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Оберіть постачальника.");
        RuleFor(x => x.SupplierSku).MaximumLength(ProductEntity.SupplierSkuMaxLength)
            .WithMessage("Артикул постачальника — до 200 символів.");
        RuleFor(x => x.SupplierUrl)
            .MaximumLength(ProductEntity.SupplierUrlMaxLength)
            .Must(BeWebUrl).When(x => !string.IsNullOrWhiteSpace(x.SupplierUrl))
            .WithMessage("Посилання на товар постачальника має починатися з https://.");
        RuleFor(x => x.SupplierPrice)
            .GreaterThan(0).LessThan(1_000_000_000m)
            .Must(value => decimal.Round(value!.Value, 2) == value).When(x => x.SupplierPrice is not null)
            .WithMessage("Ціна постачальника — більше 0, до 2 знаків після коми.");
        RuleFor(x => x.RuleCode).MaximumLength(ProductEntity.PricingRuleCodeMaxLength)
            .WithMessage("Правило ціноутворення — до 200 символів.");
        RuleFor(x => x.Unit).MaximumLength(ProductEntity.UnitMaxLength).WithMessage("Одиниця виміру — до 40 символів.");
        RuleFor(x => x.ExpectedOfferVersionId).NotEqual(Guid.Empty).When(x => x.ExpectedOfferVersionId is not null);
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
    }

    private static bool BeWebUrl(string? value)
        => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
