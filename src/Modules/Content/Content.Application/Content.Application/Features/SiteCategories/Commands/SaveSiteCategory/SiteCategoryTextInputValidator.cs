using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

/// <summary>Shape limits of the texts of one language (CNT-003 R-01).</summary>
public sealed class SiteCategoryTextInputValidator : AbstractValidator<SiteCategoryTextInput>
{
    public SiteCategoryTextInputValidator(string language)
    {
        RuleFor(x => x.Name).MaximumLength(SiteCategoryRules.NameMaxLength)
            .WithMessage($"{language}: назва — не більше {SiteCategoryRules.NameMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.Count <= SiteCategoryRules.MaxBlocks)
            .WithMessage($"{language}: текст — не більше {SiteCategoryRules.MaxBlocks} блоків.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку тексту.");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= SiteCategoryRules.TextMaxLength)
            .WithMessage($"{language}: текст — не більше {SiteCategoryRules.TextMaxLength} символів.");
    }
}
