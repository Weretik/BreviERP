using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

namespace Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

public sealed class ImportOldSiteCategoriesValidator : AbstractValidator<ImportOldSiteCategoriesCommand>
{
    public ImportOldSiteCategoriesValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteCategoriesCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteCategoriesCommand.MaxItems} розділів.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Slug).Must(ProductPageRules.IsValidSlug).WithMessage("Розділ у файлі має некоректну адресу.");
            item.RuleFor(x => x.ParentSlug).Must(slug => slug is null || ProductPageRules.IsValidSlug(slug))
                .WithMessage("Розділ у файлі має некоректну адресу батьківського розділу.");
            item.RuleFor(x => x.ProductUrls).Must(urls => urls is null || (urls.Count <= ImportOldSiteCategoriesCommand.MaxProducts
                    && urls.All(url => url is { Length: > 0 and <= ImportOldSiteCategoriesCommand.UrlMaxLength })))
                .WithMessage($"У розділі не більше {ImportOldSiteCategoriesCommand.MaxProducts} товарів із посиланнями.");
            item.RuleFor(x => x.Uk!).SetValidator(new SiteCategoryTextInputValidator("Українська")).When(x => x.Uk is not null);
            item.RuleFor(x => x.Ru!).SetValidator(new SiteCategoryTextInputValidator("Російська")).When(x => x.Ru is not null);
        }).When(x => x.Items is not null);
    }
}
