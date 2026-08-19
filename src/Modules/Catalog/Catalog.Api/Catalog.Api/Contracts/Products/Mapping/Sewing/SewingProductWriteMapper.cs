using Catalog.Application.Features.Product.Create.DTOs;

namespace Catalog.Api.Contracts.Products;

internal static class SewingProductWriteMapper
{
    public static bool HasValues(
        decimal? metersPerProduct,
        IReadOnlyCollection<FabricWriteRequest>? fabrics,
        IReadOnlyCollection<AccessoryWriteRequest>? accessories,
        IReadOnlyCollection<int>? operationIds)
        => metersPerProduct.HasValue || fabrics is not null || accessories is not null || operationIds is not null;

    public static SewingProductRequest Map(
        decimal? metersPerProduct,
        IReadOnlyCollection<FabricWriteRequest>? fabrics,
        IReadOnlyCollection<AccessoryWriteRequest>? accessories,
        IReadOnlyCollection<int>? operationIds)
        => new(
            metersPerProduct ?? 0,
            fabrics?.Select(x => new FabricRequest(x.FabricId, x.IsPrimary, x.SortOrder)).ToArray()!,
            accessories?.Select(x => new AccessoryRequest(x.GarmentAccessoryId, x.Quantity, x.SortOrder)).ToArray()!,
            operationIds!);
}
