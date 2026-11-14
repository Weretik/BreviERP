using Production.Api.Access;
using Production.Api.Endpoints.Orders.CopyProductionOrder;
using Production.Api.Endpoints.Orders.CreateProductionOrder;
using Production.Api.Endpoints.Orders.DeleteProductionOrder;
using Production.Api.Endpoints.Orders.GetProductionColors;
using Production.Api.Endpoints.Orders.GetProductionEmbroideries;
using Production.Api.Endpoints.Orders.GetProductionModelSummary;
using Production.Api.Endpoints.Orders.GetProductionOrder;
using Production.Api.Endpoints.Orders.GetProductionOrderFilterOptions;
using Production.Api.Endpoints.Orders.GetProductionOrderHistory;
using Production.Api.Endpoints.Orders.GetProductionOrders;
using Production.Api.Endpoints.Orders.GetProductionQuoteDraft;
using Production.Api.Endpoints.Orders.GetProductionQuoteSources;
using Production.Api.Endpoints.Orders.ImportProductionOrders;
using Production.Api.Endpoints.Orders.UpdateProductionOrder;

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
