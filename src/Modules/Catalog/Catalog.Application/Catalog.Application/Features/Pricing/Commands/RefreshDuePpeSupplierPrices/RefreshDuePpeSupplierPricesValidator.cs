namespace Catalog.Application.Features.Pricing.Commands.RefreshDuePpeSupplierPrices;

public sealed class RefreshDuePpeSupplierPricesValidator : AbstractValidator<RefreshDuePpeSupplierPricesCommand>
{
    public RefreshDuePpeSupplierPricesValidator() => RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
}
