namespace Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;

public sealed class PreviewPpePricesValidator : AbstractValidator<PreviewPpePricesQuery>
{
    public PreviewPpePricesValidator()
    {
        RuleFor(x => x.RuleCode).NotEmpty().WithMessage("Оберіть правило націнки.");
        RuleFor(x => x.SupplierPrice).GreaterThan(0).LessThan(10_000_000)
            .WithMessage("Ціна постачальника має бути більша за 0 і менша за 10 000 000.");
    }
}
