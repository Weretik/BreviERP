namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

public sealed class GetPricingReferencesByIdsValidator : AbstractValidator<GetPricingReferencesByIdsQuery>
{
    public GetPricingReferencesByIdsValidator()
    {
        RuleFor(x => x.FabricIds).NotNull();
        RuleFor(x => x.GarmentAccessoryIds).NotNull();
        RuleFor(x => x.GarmentPartOperationIds).NotNull();
        RuleFor(x => x.PricingParameterIds).NotNull();
        RuleFor(x => x.PricingParameterCodes).NotNull();
    }
}
