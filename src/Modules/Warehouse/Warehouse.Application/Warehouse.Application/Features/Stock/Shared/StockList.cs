
namespace Warehouse.Application.Features.Stock.Shared;

internal static class StockList
{
    public static FabricStockPermissions Permissions(WarehouseActor actor) => new(actor.CanMoveStock, actor.SeesStockValue,
        actor.CanImport);
}
