namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed class GetPricingReferencesByIdsQueryValidator : AbstractValidator<GetPricingReferencesByIdsQuery>
{
    public GetPricingReferencesByIdsQueryValidator()
    {
        RuleFor(x => x.FabricIds).NotNull();
        RuleFor(x => x.GarmentAccessoryIds).NotNull();
        RuleFor(x => x.GarmentPartOperationIds).NotNull();
        RuleFor(x => x.PricingParameterIds).NotNull();
        RuleFor(x => x.PricingParameterCodes).NotNull();
    }
}
