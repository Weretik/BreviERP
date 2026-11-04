namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Refresh.Validators;

public sealed class RefreshPpeSupplierPricesCommandValidator : AbstractValidator<RefreshPpeSupplierPricesCommand>
{
    public RefreshPpeSupplierPricesCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ActorName).NotEmpty().MaximumLength(200);
    }
}
