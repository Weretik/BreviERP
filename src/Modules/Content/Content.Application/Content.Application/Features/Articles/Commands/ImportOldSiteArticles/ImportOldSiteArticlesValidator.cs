using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Commands.ImportOldSiteArticles.Results;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

public sealed class ImportOldSiteArticlesValidator : AbstractValidator<ImportOldSiteArticlesCommand>
{
    public ImportOldSiteArticlesValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteArticlesCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteArticlesCommand.MaxItems} статей.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Slug).Must(ProductPageRules.IsValidSlug).WithMessage("Стаття у файлі має некоректну адресу.");
            item.RuleFor(x => x.PublishedOn).Must(date => date.Year is >= 2000 and <= 2100)
                .WithMessage("Стаття у файлі має некоректну дату.");
            item.RuleFor(x => x.CoverUrl).Must(url => url is null || OldSiteArticlePhotos.IsValid(url))
                .WithMessage("Обкладинка статті у файлі має бути фото зі старого сайту (https, JPG, PNG або WebP).");
            item.RuleFor(x => x.Uk!).SetValidator(new OldSiteArticleTextValidator("Українська")).When(x => x.Uk is not null);
            item.RuleFor(x => x.Ru!).SetValidator(new OldSiteArticleTextValidator("Російська")).When(x => x.Ru is not null);
        }).When(x => x.Items is not null);
    }
}
