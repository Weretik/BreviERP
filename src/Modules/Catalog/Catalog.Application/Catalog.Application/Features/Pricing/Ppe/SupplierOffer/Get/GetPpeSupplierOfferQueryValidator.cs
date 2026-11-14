using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Get;

public sealed class GetPpeSupplierOfferQueryValidator : AbstractValidator<GetPpeSupplierOfferQuery>
{
    public GetPpeSupplierOfferQueryValidator() => RuleFor(x => x.ProductId).ValidProductId();
}
