using BuildingBlocks.Domain.Exceptions;
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

    public static RetailPricePercent FromReference(int additionalReferenceId)
    {
        if (additionalReferenceId <= 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(additionalReferenceId)));
        return new RetailPricePercent(PricePercentSource.Reference, additionalReferenceId, null);
    }

    public static RetailPricePercent FromCustom(decimal customPercent)
    {
        if (customPercent < 0) throw new DomainException(ProductErrors.PercentageConfigurationInvalid(nameof(customPercent)));
        return new RetailPricePercent(PricePercentSource.Custom, null, customPercent);
    }
}
