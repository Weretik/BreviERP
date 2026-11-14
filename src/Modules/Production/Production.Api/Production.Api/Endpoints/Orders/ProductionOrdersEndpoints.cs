using Production.Api.Access;

namespace Production.Api.Endpoints.Orders;

/// <summary>«Виробничі завдання»: orders the workshop sews, created from КП «під запит».</summary>
internal static class ProductionOrdersEndpoints
{
    public static IEndpointRouteBuilder MapProductionOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/orders").RequireAuthorization(ProductionAccess.Orders)
            .WithTags("ProductionOrders");
        GetProductionOrdersEndpoint.Map(group);
        GetProductionModelSummaryEndpoint.Map(group);
        GetProductionOrderFilterOptionsEndpoint.Map(group);
        GetProductionOrderEndpoint.Map(group);
        GetProductionOrderHistoryEndpoint.Map(group);
        GetProductionQuoteSourcesEndpoint.Map(group);
        GetProductionQuoteDraftEndpoint.Map(group);
        GetProductionColorsEndpoint.Map(group);
        GetProductionEmbroideriesEndpoint.Map(group);
        CreateProductionOrderEndpoint.Map(group);
        UpdateProductionOrderEndpoint.Map(group);
        CopyProductionOrderEndpoint.Map(group);
        DeleteProductionOrderEndpoint.Map(group);
        ImportProductionOrdersEndpoint.Map(group);
        return app;
    }
}
