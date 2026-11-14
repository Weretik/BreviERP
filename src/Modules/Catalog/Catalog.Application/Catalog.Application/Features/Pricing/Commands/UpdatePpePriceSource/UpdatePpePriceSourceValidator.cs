namespace Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;

public sealed class UpdatePpePriceSourceValidator : AbstractValidator<UpdatePpePriceSourceCommand>
{
    public UpdatePpePriceSourceValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(40);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0m, 90m)
            .Must(value => decimal.Round(value, 2) == value)
            .WithMessage("Знижка — від 0 до 90%, до 2 знаків після коми.");
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId is not null);
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
    }
}
