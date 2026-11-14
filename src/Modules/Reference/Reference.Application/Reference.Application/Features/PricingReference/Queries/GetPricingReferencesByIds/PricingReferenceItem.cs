namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

public sealed record PricingReferenceItem(
    int Id,
    decimal Value,
    string? Key = null,
    string? Unit = null);
