namespace Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;

public sealed class ImportOldSitePhotosValidator : AbstractValidator<ImportOldSitePhotosCommand>
{
    public ImportOldSitePhotosValidator()
    {
        RuleFor(x => x.BaseUrl)
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && x.EndsWith('/'))
            .WithMessage("Адреса фото старого сайту має починатися з https:// і закінчуватися на /.");
        RuleFor(x => x.Actor).NotEmpty();
        RuleFor(x => x.Products).NotEmpty().WithMessage("Файл не має товарів.");
        RuleFor(x => x.Products.Count)
            .LessThanOrEqualTo(ImportOldSitePhotosCommand.MaxProductsPerRun)
            .When(x => !x.DryRun && x.Products is not null)
            .WithMessage($"За один запит — не більше {ImportOldSitePhotosCommand.MaxProductsPerRun} товарів.");
        RuleFor(x => x.Products.Count)
            .LessThanOrEqualTo(ImportOldSitePhotosCommand.MaxProductsPerDryRun)
            .When(x => x.DryRun && x.Products is not null)
            .WithMessage($"У файлі — не більше {ImportOldSitePhotosCommand.MaxProductsPerDryRun} товарів.");
        RuleForEach(x => x.Products).ChildRules(product =>
        {
            product.RuleFor(x => x.OldId).GreaterThan(0);
            product.RuleFor(x => x.OldName).NotNull().MaximumLength(300);
            product.RuleFor(x => x.Skus).NotNull();
            product.RuleForEach(x => x.Skus).NotEmpty().MaximumLength(80);
            product.RuleFor(x => x.Photos).NotNull().Must(x => x.Count <= 40).WithMessage("Не більше 40 фото в товарі.");
            product.RuleForEach(x => x.Photos).ChildRules(photo =>
                photo.RuleFor(x => x.Path).NotEmpty().MaximumLength(400)
                    .Must(OldSitePhotoPaths.IsSafe).WithMessage("Шлях до фото некоректний."));
            product.When(x => x.NewModel is not null, () =>
            {
                product.RuleFor(x => x.NewModel!.Name).NotEmpty().MaximumLength(200);
                product.RuleFor(x => x.NewModel!.Section).NotEmpty().MaximumLength(200);
                product.RuleFor(x => x.NewModel!.Season)
                    .Must(x => x is null || Domain.Products.Product.Seasons.Contains(x))
                    .WithMessage("Сезон: «Літо», «Зима» або порожньо.");
            });
        });
    }
}
