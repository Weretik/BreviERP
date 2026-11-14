using Content.Application.Abstractions;
using Content.Domain.Articles;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Articles.Commands.SaveContentArticle;

/// <summary>Shape limits of the texts of one language (CNT-004 R-01).</summary>
public sealed class ArticleTextInputValidator : AbstractValidator<ArticleTextInput>
{
    public ArticleTextInputValidator(string language)
    {
        RuleFor(x => x.Title).MaximumLength(ArticleRules.TitleMaxLength)
            .WithMessage($"{language}: назва — не більше {ArticleRules.TitleMaxLength} символів.");
        RuleFor(x => x.Excerpt).MaximumLength(ArticleRules.ExcerptMaxLength)
            .WithMessage($"{language}: короткий опис — не більше {ArticleRules.ExcerptMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Count <= ArticleRules.MaxBlocks)
            .WithMessage($"{language}: текст — не більше {ArticleRules.MaxBlocks} блоків.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку тексту.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                block.Kind != ArticleBlockKind.Image || block.PhotoId is > 0))
            .WithMessage($"{language}: у блоку з фото не обрано фото.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).All(block =>
                (block.Alt?.Length ?? 0) <= ArticleRules.AltMaxLength))
            .WithMessage($"{language}: підпис фото — не більше {ArticleRules.AltMaxLength} символів.");
        RuleFor(x => x.Body).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= ArticleRules.TextMaxLength)
            .WithMessage($"{language}: текст — не більше {ArticleRules.TextMaxLength} символів.");
    }
}
