namespace Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;

public sealed class RefreshPpeSupplierPricesValidator : AbstractValidator<RefreshPpeSupplierPricesCommand>
{
    public RefreshPpeSupplierPricesValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ActorName).NotEmpty().MaximumLength(200);
    }
}
