using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Articles.Commands.SaveContentArticle;

public sealed class SaveContentArticleValidator : AbstractValidator<SaveContentArticleCommand>
{
    public SaveContentArticleValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Slug).Must(slug => string.IsNullOrWhiteSpace(slug) || ProductPageRules.IsValidSlug(slug.Trim()))
            .WithMessage(ArticleMessages.SlugInvalid);
        RuleFor(x => x.PublishedOn).Must(date => date.Year is >= 2000 and <= 2100).WithMessage("Вкажіть дату статті.");
        RuleFor(x => x.CoverPhotoId).GreaterThan(0).When(x => x.CoverPhotoId is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Uk).Must(uk => ProductPageRules.Clean(uk?.Title) is not null)
            .WithMessage(ArticleMessages.TitleRequired);
        RuleFor(x => x.Uk!).SetValidator(new ArticleTextInputValidator("Українська")).When(x => x.Uk is not null);
        RuleFor(x => x.Ru!).SetValidator(new ArticleTextInputValidator("Російська")).When(x => x.Ru is not null);
    }
}
