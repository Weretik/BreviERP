using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Update.Validators;

public sealed class UpdateProductOverviewCommandValidator : AbstractValidator<UpdateProductOverviewCommand>
{
    public UpdateProductOverviewCommandValidator()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000);
        RuleFor(x => x.Code).MaximumLength(ProductEntity.CodeMaxLength).WithMessage("Код — до 80 символів.");
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
        RuleFor(x => x.Season)
            .Must(value => ProductEntity.Seasons.Contains(value!.Trim()))
            .When(x => !string.IsNullOrWhiteSpace(x.Season))
            .WithMessage("Сезон: «Літо», «Зима» або порожньо.");
        RuleFor(x => x.CurrentSiteUrl)
            .MaximumLength(ProductEntity.CurrentSiteUrlMaxLength)
            .Must(BeWebUrl).When(x => !string.IsNullOrWhiteSpace(x.CurrentSiteUrl))
            .WithMessage("Посилання на сайт має починатися з https:// і бути до 500 символів.");
        RuleFor(x => x.QuoteDescription)
            .MaximumLength(ProductEntity.QuoteDescriptionMaxLength)
            .WithMessage("Опис для КП — до 2000 символів.");
    }

    private static bool BeWebUrl(string? value)
        => Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
