using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.ValueObjects;

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

    public static WholesalePricePercent FromReference(int additionalReferenceId)
    {
        if (additionalReferenceId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(additionalReferenceId)));
        return new WholesalePricePercent(PricePercentSource.Reference, additionalReferenceId, null);
    }

    public static WholesalePricePercent FromCustom(decimal customPercent)
    {
        if (customPercent < 0) throw new DomainException(ProductErrors.PercentageConfigurationInvalid(nameof(customPercent)));
        return new WholesalePricePercent(PricePercentSource.Custom, null, customPercent);
    }
}
