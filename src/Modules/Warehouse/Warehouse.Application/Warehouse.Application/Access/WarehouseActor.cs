namespace Warehouse.Application.Access;

/// <summary>
/// The signed-in person and what they may do on «Склад тканин». Admin (the director also holds Admin) and the head of
/// the workshop move the stock and see its value; a Manager only looks.
/// </summary>
public sealed record WarehouseActor(int UserId, bool IsAdmin, bool IsManager, bool IsWorkshopChief)
{
    /// <summary>Receipts, corrections, reversals and issues (PRD-D31).</summary>
    public bool CanMoveStock => IsAdmin || IsWorkshopChief;

    /// <summary>The stock value in UAH.</summary>
    public bool SeesStockValue => IsAdmin || IsWorkshopChief;

    /// <summary>The transfer from the old «Цех».</summary>
    public bool CanImport => IsAdmin;

    /// <summary>«Видача» to an item: the stock keepers on an open («Нове») order.</summary>
    public bool CanIssueFabric(Abstractions.WarehousePlanItem item) => CanMoveStock && item.IsOpen;
}

public static class WarehouseActorRules
{
    public static IRuleBuilderOptions<T, WarehouseActor> ValidActor<T>(this IRuleBuilder<T, WarehouseActor> rule) =>
        rule.NotNull().Must(x => x is null || x.UserId > 0);
}
