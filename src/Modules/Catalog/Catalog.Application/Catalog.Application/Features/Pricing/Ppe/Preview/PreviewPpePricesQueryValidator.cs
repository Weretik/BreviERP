namespace Catalog.Application.Features.Pricing.Ppe.Preview;

public sealed class PreviewPpePricesQueryValidator : AbstractValidator<PreviewPpePricesQuery>
{
    public PreviewPpePricesQueryValidator()
    {
        RuleFor(x => x.RuleCode).NotEmpty().WithMessage("Оберіть правило націнки.");
        RuleFor(x => x.SupplierPrice).GreaterThan(0).LessThan(10_000_000)
            .WithMessage("Ціна постачальника має бути більша за 0 і менша за 10 000 000.");
    }
}
