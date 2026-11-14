using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.ProductPages.Commands.SaveContentProduct;

/// <summary>Shape limits of the texts of one language (CNT-002 R-04).</summary>
public sealed class ProductPageTextInputValidator : AbstractValidator<ProductPageTextInput>
{
    public ProductPageTextInputValidator(string language)
    {
        RuleFor(x => x.Name).MaximumLength(ProductPageRules.NameMaxLength)
            .WithMessage($"{language}: назва — не більше {ProductPageRules.NameMaxLength} символів.");
        RuleFor(x => x.SeoTitle).MaximumLength(ProductPageRules.SeoTitleMaxLength)
            .WithMessage($"{language}: SEO-заголовок — не більше {ProductPageRules.SeoTitleMaxLength} символів.");
        RuleFor(x => x.SeoDescription).MaximumLength(ProductPageRules.SeoDescriptionMaxLength)
            .WithMessage($"{language}: SEO-опис — не більше {ProductPageRules.SeoDescriptionMaxLength} символів.");

        RuleFor(x => x.Summary).Must(facts => facts is null || facts.Count <= ProductPageRules.MaxFacts)
            .WithMessage($"{language}: короткі відомості — не більше {ProductPageRules.MaxFacts} рядків.");
        RuleFor(x => x.Summary).Must(facts => facts is null || facts.All(fact =>
                fact is not null && (fact.Label?.Length ?? 0) <= ProductPageRules.FactLabelMaxLength
                                 && (fact.Text?.Length ?? 0) <= ProductPageRules.FactTextMaxLength))
            .WithMessage($"{language}: рядок коротких відомостей задовгий.");

        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.Count <= ProductPageRules.MaxBlocks)
            .WithMessage($"{language}: опис — не більше {ProductPageRules.MaxBlocks} блоків.");
        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.All(block =>
                block is not null && Enum.IsDefined(block.Kind)))
            .WithMessage($"{language}: невідомий тип блоку опису.");
        RuleFor(x => x.Description).Must(blocks => blocks is null || blocks.Where(block => block is not null).Sum(block =>
                (block.Text?.Length ?? 0) + (block.Lead?.Length ?? 0) + (block.Items?.Sum(item => item?.Length ?? 0) ?? 0))
            <= ProductPageRules.DescriptionMaxLength)
            .WithMessage($"{language}: опис — не більше {ProductPageRules.DescriptionMaxLength} символів.");

        RuleFor(x => x.Specs).Must(groups => groups is null || groups.Count <= ProductPageRules.MaxSpecGroups)
            .WithMessage($"{language}: характеристики — не більше {ProductPageRules.MaxSpecGroups} груп.");
        RuleFor(x => x.Specs).Must(groups => groups is null || groups.All(group =>
                group is not null && (group.Title?.Length ?? 0) <= ProductPageRules.SpecTitleMaxLength
                                  && (group.Rows?.Count ?? 0) <= ProductPageRules.MaxSpecRows
                                  && (group.Rows ?? []).All(row =>
                                      row is not null && (row.Name?.Length ?? 0) <= ProductPageRules.SpecNameMaxLength
                                                      && (row.Value?.Length ?? 0) <= ProductPageRules.SpecValueMaxLength)))
            .WithMessage($"{language}: група або рядок характеристик задовгі.");
    }
}
