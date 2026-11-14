using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;

namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

public sealed class ImportOldSiteProductPagesValidator : AbstractValidator<ImportOldSiteProductPagesCommand>
{
    public ImportOldSiteProductPagesValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteProductPagesCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteProductPagesCommand.MaxItems} сторінок.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Url).NotEmpty().MaximumLength(ImportOldSiteProductPagesCommand.UrlMaxLength);
            item.RuleFor(x => x.UrlRu).MaximumLength(ImportOldSiteProductPagesCommand.UrlMaxLength);
            item.RuleFor(x => x.Uk!).SetValidator(new ProductPageTextInputValidator("Українська")).When(x => x.Uk is not null);
            item.RuleFor(x => x.Ru!).SetValidator(new ProductPageTextInputValidator("Російська")).When(x => x.Ru is not null);
        }).When(x => x.Items is not null);
    }
}
