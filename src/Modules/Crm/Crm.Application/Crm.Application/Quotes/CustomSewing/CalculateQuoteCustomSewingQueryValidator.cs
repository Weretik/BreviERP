using Crm.Application.Quotes.Validation;

namespace Crm.Application.Quotes.CustomSewing;

public sealed class CalculateQuoteCustomSewingQueryValidator : AbstractValidator<CalculateQuoteCustomSewingQuery>
{
    public CalculateQuoteCustomSewingQueryValidator()
    {
        RuleFor(x => x.Input).NotNull().ChildRules(input =>
        {
            input.RuleFor(x => x.Fabrics).NotNull()
                .Must(x => x is { Count: <= Domain.Quotes.QuoteLimits.MaxOptionsPerItem })
                .WithMessage($"Не більше {Domain.Quotes.QuoteLimits.MaxOptionsPerItem} тканин.");
            input.RuleForEach(x => x.Fabrics).NotNull().ChildRules(fabric =>
            {
                fabric.RuleFor(x => x.FabricId).GreaterThan(0);
                fabric.RuleFor(x => x.Meters).Must(QuoteCompositionRules.IsQuantity)
                    .WithMessage(QuoteCompositionRules.MetersMessage);
            });
            input.RuleFor(x => x.Lines).NotNull()
                .Must(x => x is { Count: <= Domain.Quotes.QuoteLimits.MaxCompositionLines })
                .WithMessage(QuoteCompositionRules.TooManyLinesMessage);
            input.RuleForEach(x => x.Lines).NotNull().SetValidator(new QuoteItemLineInputValidator());
        });
    }
}
