using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Api.Contracts.Products;

internal static class PpeProductWriteMapper
{
    public static bool HasValues(
        int? supplierId,
        decimal? basePrice,
        PercentWriteRequest? retailPercent,
        PercentWriteRequest? wholesalePercent)
        => supplierId.HasValue || basePrice.HasValue || retailPercent is not null || wholesalePercent is not null;

    public static PpeProductRequest Map(
        int? supplierId,
        decimal? basePrice,
        PercentWriteRequest? retailPercent,
        PercentWriteRequest? wholesalePercent)
        => new(supplierId ?? 0, basePrice ?? 0, MapPercent(retailPercent), MapPercent(wholesalePercent));

    private static PercentRequest MapPercent(PercentWriteRequest? request)
    {
        var source = request is not null && Enum.TryParse<PricePercentSource>(request.Source, true, out var parsed)
            ? parsed
            : (PricePercentSource)(-1);
        return new PercentRequest(source, request?.AdditionalReferenceId, request?.CustomPercent);
    }
}
