using Warehouse.Api.Access;

namespace Warehouse.Api.Endpoints.OrderItemFabric;

/// <summary>Tab «Тканина» of an item of «Виробничі завдання»: plan by colour, «Видати» and «Сторно видачі».</summary>
internal static class OrderItemFabricEndpoints
{
    public static IEndpointRouteBuilder MapOrderItemFabricEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/warehouse/orders").RequireAuthorization(WarehouseAccess.ProductionOrders)
            .WithTags("OrderItemFabric");
        GetProductionItemFabricEndpoint.Map(group);
        CreateFabricIssueEndpoint.Map(group);
        ReverseFabricIssueEndpoint.Map(group);
        return app;
    }
}
