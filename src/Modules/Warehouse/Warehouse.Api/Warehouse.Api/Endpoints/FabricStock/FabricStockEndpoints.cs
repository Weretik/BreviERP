using Warehouse.Api.Access;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>
/// «Склад тканин»: «Залишки», «Рух тканини», «Початкові залишки», the operations of a fabric and colour pair, the period
/// summary and the transfer from the old «Цех».
/// </summary>
internal static class FabricStockEndpoints
{
    public static IEndpointRouteBuilder MapFabricStockEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/warehouse/fabric-stock").RequireAuthorization(WarehouseAccess.FabricStock)
            .WithTags("FabricStock");
        GetFabricStockEndpoint.Map(group);
        DeleteFabricStockEndpoint.Map(group);
        GetFabricStockChoicesEndpoint.Map(group);
        GetFabricMovementsEndpoint.Map(group);
        OpenFabricStockEndpoint.Map(group);
        ReceiveFabricEndpoint.Map(group);
        IssueFabricEndpoint.Map(group);
        AdjustFabricStockEndpoint.Map(group);
        ReverseFabricMovementEndpoint.Map(group);
        EditFabricMovementEndpoint.Map(group);
        VoidFabricMovementEndpoint.Map(group);
        RestoreFabricMovementEndpoint.Map(group);
        GetFabricMovementHistoryEndpoint.Map(group);
        GetFabricIssueItemsEndpoint.Map(group);
        GetFabricIssueItemEndpoint.Map(group);
        IssueFabricToItemEndpoint.Map(group);
        RefreshFabricItemPlanEndpoint.Map(group);
        GetFabricConsumptionEndpoint.Map(group);
        TransferFabricStockEndpoint.Map(group);
        return app;
    }
}
