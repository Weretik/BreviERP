using Warehouse.Application.Access;
using Warehouse.Application.Contracts;

namespace Warehouse.Application.Stock;

internal static class StockList
{
    public static FabricStockPermissions Permissions(WarehouseActor actor) => new(actor.CanMoveStock, actor.SeesStockValue);
}
