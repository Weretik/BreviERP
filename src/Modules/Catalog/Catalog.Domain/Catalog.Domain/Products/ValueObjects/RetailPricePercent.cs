using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.ValueObjects;

public sealed class RetailPricePercent
{
    public PricePercentSource Source { get; private set; }
    public int? AdditionalReferenceId { get; private set; }
    public decimal? CustomPercent { get; private set; }

    private RetailPricePercent() { }

    private RetailPricePercent(PricePercentSource source, int? additionalReferenceId, decimal? customPercent)
    {
        Source = source;
        AdditionalReferenceId = additionalReferenceId;
        CustomPercent = customPercent;
    }

    public static DomainResult<RetailPricePercent> FromReference(int additionalReferenceId)
    {
        if (additionalReferenceId <= 0) return ProductErrors.PositiveValueRequired(nameof(additionalReferenceId));
        return new RetailPricePercent(PricePercentSource.Reference, additionalReferenceId, null);
    }

    public static DomainResult<RetailPricePercent> FromCustom(decimal customPercent)
    {
        if (customPercent < 0) return ProductErrors.PercentageConfigurationInvalid(nameof(customPercent));
        return new RetailPricePercent(PricePercentSource.Custom, null, customPercent);
    }
}
