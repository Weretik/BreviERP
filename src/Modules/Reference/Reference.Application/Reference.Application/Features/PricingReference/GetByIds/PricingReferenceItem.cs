namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed record PricingReferenceItem(
    int Id,
    decimal Value,
    string? Key = null,
    string? Unit = null);
