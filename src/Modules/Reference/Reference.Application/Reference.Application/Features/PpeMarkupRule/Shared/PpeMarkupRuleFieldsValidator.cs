using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Shared;

public sealed class PpeMarkupRuleFieldsValidator : AbstractValidator<IPpeMarkupRuleFields>
{
    private const string MarkupRange = "Націнка має бути від 0 до 1000 %.";

    public PpeMarkupRuleFieldsValidator()
    {
        RuleFor(x => x.RetailMarkup).InclusiveBetween(0, PpeMarkupRuleEntity.MaxMarkup).WithMessage(MarkupRange);
        RuleFor(x => x.WholesaleMarkup).InclusiveBetween(0, PpeMarkupRuleEntity.MaxMarkup).WithMessage(MarkupRange);
        RuleFor(x => x.LargeWholesaleMarkup).InclusiveBetween(0, PpeMarkupRuleEntity.MaxMarkup).WithMessage(MarkupRange);

        RuleFor(x => x.PriceNote)
            .MaximumLength(PpeMarkupRuleEntity.PriceNoteMaxLength).WithMessage("Примітка для прайсу: не більше 500 символів.");
        RuleFor(x => x.AppliesTo)
            .MaximumLength(PpeMarkupRuleEntity.AppliesToMaxLength).WithMessage("«До яких товарів належить»: не більше 500 символів.");
        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 100_000).WithMessage("Порядок має бути від 0 до 100000.");
    }
}
