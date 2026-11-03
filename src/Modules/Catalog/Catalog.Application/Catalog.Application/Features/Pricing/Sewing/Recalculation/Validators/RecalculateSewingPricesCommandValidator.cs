namespace Catalog.Application.Features.Pricing.Sewing.Recalculation.Validators;

public sealed class RecalculateSewingPricesCommandValidator : AbstractValidator<RecalculateSewingPricesCommand>
{
    public RecalculateSewingPricesCommandValidator()
    {
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductIds).Must(ids => ids is null || (ids.Count is >= 1 and <= 500 && ids.All(id => id > 0)))
            .WithMessage("Provide 1–500 positive Product IDs, or none for every Sewing model.");
    }
}
