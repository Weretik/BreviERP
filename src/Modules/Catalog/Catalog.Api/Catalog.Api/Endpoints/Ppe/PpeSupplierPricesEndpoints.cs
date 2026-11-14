namespace Catalog.Api.Endpoints.Ppe;

/// <summary>«Довідники» → «Імпорт цін»: PPE supplier price sources, their discounts, file imports and «Оновити зараз» (CAT-017).</summary>
internal static class PpeSupplierPricesEndpoints
{
    public static IEndpointRouteBuilder MapPpeSupplierPricesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/pricing/ppe/supplier-prices")
            .WithTags("PpeSupplierPrices");
        ListPpePriceSourcesEndpoint.Map(group);
        UpdatePpePriceSourceEndpoint.Map(group);
        ImportPpeSupplierPricesEndpoint.Map(group);
        RefreshPpeSupplierPricesEndpoint.Map(group);
        return app;
    }
}
