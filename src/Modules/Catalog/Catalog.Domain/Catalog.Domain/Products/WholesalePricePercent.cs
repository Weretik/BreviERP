
namespace Catalog.Domain.Products;

public sealed class WholesalePricePercent
{
    public PricePercentSource Source { get; private set; }
    public int? AdditionalReferenceId { get; private set; }
    public decimal? CustomPercent { get; private set; }

    private WholesalePricePercent() { }

    private WholesalePricePercent(PricePercentSource source, int? additionalReferenceId, decimal? customPercent)
    {
        Source = source;
        AdditionalReferenceId = additionalReferenceId;
        CustomPercent = customPercent;
    }

    public static DomainResult<WholesalePricePercent> FromReference(int additionalReferenceId)
    {
        if (additionalReferenceId <= 0) return ProductErrors.PositiveValueRequired(nameof(additionalReferenceId));
        return new WholesalePricePercent(PricePercentSource.Reference, additionalReferenceId, null);
    }

    public static DomainResult<WholesalePricePercent> FromCustom(decimal customPercent)
    {
        if (customPercent < 0) return ProductErrors.PercentageConfigurationInvalid(nameof(customPercent));
        return new WholesalePricePercent(PricePercentSource.Custom, null, customPercent);
    }
}
