using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Commands.CreateCatalogProduct;

public sealed class CreateCatalogProductValidator : AbstractValidator<CreateCatalogProductCommand>
{
    public CreateCatalogProductValidator()
    {
        RuleFor(x => x.Type).Must(x => x is "Sewing" or "Ppe").WithMessage("Тип товару: Sewing або Ppe.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву.")
            .MaximumLength(200).WithMessage("Назва — до 200 символів.");
        RuleFor(x => x.Code).MaximumLength(ProductEntity.CodeMaxLength).WithMessage("Код — до 80 символів.");
        RuleFor(x => x.Code).NotEmpty().When(x => x.Type == "Ppe").WithMessage("Вкажіть артикул на сайті.");
        RuleFor(x => x.SectionId).GreaterThan(0).WithMessage("Оберіть розділ.");
        RuleFor(x => x.Season)
            .Must(value => ProductEntity.Seasons.Contains(value!.Trim()))
            .When(x => !string.IsNullOrWhiteSpace(x.Season))
            .WithMessage("Сезон: «Літо», «Зима» або порожньо.");
        RuleFor(x => x.Season).Empty().When(x => x.Type == "Ppe").WithMessage("Сезон буває тільки у швейки.");
        RuleFor(x => x.CurrentSiteUrl)
            .MaximumLength(ProductEntity.CurrentSiteUrlMaxLength)
            .Must(BeWebUrl).When(x => !string.IsNullOrWhiteSpace(x.CurrentSiteUrl))
            .WithMessage("Посилання на сайт має починатися з https:// і бути до 500 символів.");
        RuleFor(x => x.QuoteDescription).MaximumLength(ProductEntity.QuoteDescriptionMaxLength)
            .WithMessage("Опис для КП — до 2000 символів.");
        RuleFor(x => x.Actor).NotEmpty();

        RuleFor(x => x.Ppe).NotNull().When(x => x.Type == "Ppe").WithMessage("Для ЗІЗ потрібні постачальник і ціна.");
        RuleFor(x => x.Ppe).Null().When(x => x.Type == "Sewing").WithMessage("Швейка не має постачальника.");
        When(x => x.Ppe is not null, () =>
        {
            RuleFor(x => x.Ppe!.SupplierId).GreaterThan(0).WithMessage("Оберіть постачальника.");
            RuleFor(x => x.Ppe!.SupplierSku).MaximumLength(ProductEntity.SupplierSkuMaxLength)
                .WithMessage("Артикул постачальника — до 200 символів.");
            RuleFor(x => x.Ppe!.SupplierUrl).NotEmpty().WithMessage("Вкажіть посилання на товар постачальника.")
                .MaximumLength(ProductEntity.SupplierUrlMaxLength).Must(BeWebUrl)
                .WithMessage("Посилання постачальника має починатися з https://.");
            RuleFor(x => x.Ppe!.SupplierPrice).GreaterThan(0).WithMessage("Ціна постачальника має бути більша за 0.")
                .LessThan(10_000_000).WithMessage("Ціна постачальника завелика.")
                .Must(x => decimal.Round(x, 2) == x).WithMessage("Ціна — до 2 знаків після коми.");
            RuleFor(x => x.Ppe!.RuleCode).NotEmpty().WithMessage("Оберіть правило націнки.")
                .MaximumLength(ProductEntity.PricingRuleCodeMaxLength);
            RuleFor(x => x.Ppe!.Unit).NotEmpty().WithMessage("Вкажіть одиницю.")
                .MaximumLength(ProductEntity.UnitMaxLength).WithMessage("Одиниця — до 40 символів.");
        });
    }

    private static bool BeWebUrl(string? value)
        => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
