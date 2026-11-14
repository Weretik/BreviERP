using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;

public sealed class GetPpeSupplierOfferValidator : AbstractValidator<GetPpeSupplierOfferQuery>
{
    public GetPpeSupplierOfferValidator() => RuleFor(x => x.ProductId).ValidProductId();
}
