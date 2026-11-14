using Warehouse.Api.Access;
using Warehouse.Api.Endpoints.FabricStock.AdjustFabricStock;
using Warehouse.Api.Endpoints.FabricStock.DeleteFabricStock;
using Warehouse.Api.Endpoints.FabricStock.EditFabricMovement;
using Warehouse.Api.Endpoints.FabricStock.GetFabricConsumption;
using Warehouse.Api.Endpoints.FabricStock.GetFabricIssueItem;
using Warehouse.Api.Endpoints.FabricStock.GetFabricIssueItems;
using Warehouse.Api.Endpoints.FabricStock.GetFabricMovementHistory;
using Warehouse.Api.Endpoints.FabricStock.GetFabricMovements;
using Warehouse.Api.Endpoints.FabricStock.GetFabricStock;
using Warehouse.Api.Endpoints.FabricStock.GetFabricStockChoices;
using Warehouse.Api.Endpoints.FabricStock.IssueFabric;
using Warehouse.Api.Endpoints.FabricStock.IssueFabricToItem;
using Warehouse.Api.Endpoints.FabricStock.OpenFabricStock;
using Warehouse.Api.Endpoints.FabricStock.ReceiveFabric;
using Warehouse.Api.Endpoints.FabricStock.RefreshFabricItemPlan;
using Warehouse.Api.Endpoints.FabricStock.RestoreFabricMovement;
using Warehouse.Api.Endpoints.FabricStock.ReverseFabricMovement;
using Warehouse.Api.Endpoints.FabricStock.TransferFabricStock;
using Warehouse.Api.Endpoints.FabricStock.VoidFabricMovement;

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
