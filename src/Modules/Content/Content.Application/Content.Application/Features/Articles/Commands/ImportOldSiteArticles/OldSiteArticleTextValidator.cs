using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Commands.SaveContentArticle;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.ImportOldSiteArticles;

public sealed class OldSiteArticleTextValidator : AbstractValidator<OldSiteArticleText>
{
    public OldSiteArticleTextValidator(string language)
    {
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                block.Kind != ArticleBlockKind.Image || OldSiteArticlePhotos.IsValid(block.ImageUrl)))
            .WithMessage($"{language}: фото в тексті має бути зі старого сайту (https, JPG, PNG або WebP).");
        // The same limits as a save from the editor; a picture counts as a block with a photo.
        RuleFor(x => x.ToInput(_ => 1)).SetValidator(new ArticleTextInputValidator(language)).OverridePropertyName("Text");
    }
}
