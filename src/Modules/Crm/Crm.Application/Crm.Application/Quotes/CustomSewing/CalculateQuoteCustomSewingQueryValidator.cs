using Crm.Application.Quotes.Validation;

namespace Crm.Application.Quotes.CustomSewing;

public sealed class CalculateQuoteCustomSewingQueryValidator : AbstractValidator<CalculateQuoteCustomSewingQuery>
{
    public CalculateQuoteCustomSewingQueryValidator()
    {
        RuleFor(x => x.Input).NotNull().ChildRules(input =>
        {
            input.RuleFor(x => x.Variants).NotNull()
                .Must(x => x is { Count: <= Domain.Quotes.QuoteLimits.MaxOptionsPerItem })
                .WithMessage($"Не більше {Domain.Quotes.QuoteLimits.MaxOptionsPerItem} варіантів.");
            input.RuleForEach(x => x.Variants).NotNull().ChildRules(variant =>
                variant.RuleFor(x => x.Fabrics).SetValidator(new QuoteOptionFabricsValidator()));
            input.RuleFor(x => x.Lines).NotNull()
                .Must(x => x is { Count: <= Domain.Quotes.QuoteLimits.MaxCompositionLines })
                .WithMessage(QuoteCompositionRules.TooManyLinesMessage);
            input.RuleForEach(x => x.Lines).NotNull().SetValidator(new QuoteItemLineInputValidator());
        });
    }
}
