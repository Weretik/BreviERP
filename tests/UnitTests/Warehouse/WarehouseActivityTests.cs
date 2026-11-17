using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Warehouse.Application.Features.Activity.EventHandlers;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;

namespace UnitTests.Warehouse;

/// <summary>Fabric movements and their edits read as «Журнал дій» entries (ACT-001).</summary>
public sealed class WarehouseActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 16);

    private static FabricStock Line() => FabricStock.Create(new StockFabric(1, "tk-12", "Грета"), "Синій", Now);

    [Test]
    public void A_receipt_names_the_line_metres_and_supplier()
    {
        var movement = Line().Move(FabricMovementKind.Receipt, 12.5m,
            new FabricMovementDetails(Today, null, SupplierName: "Текстиль-Контакт"), "Комірник", Now);

        var entry = new FabricMovementActivity().Describe(((IHasDomainEvents)movement).DomainEvents.Single());

        Assert.That(entry!.SubjectTitle, Is.EqualTo("TK-12 Грета · Синій"));
        Assert.That(entry.Summary, Is.EqualTo("Прихід: +12,50 м; Текстиль-Контакт"));
    }

    [Test]
    public void Voiding_a_movement_is_listed_with_the_reason()
    {
        var line = Line();
        var movement = line.Move(FabricMovementKind.Receipt, 10m, new FabricMovementDetails(Today, null), "Комірник", Now);

        var revision = line.Revise(movement, movement.State with { Delta = 0, IsVoided = true },
            FabricMovementRevisionAction.Void, "Помилковий прихід", "Комірник", Now, Guid.NewGuid()).Value;

        var entry = new FabricRevisionActivity().Describe(((IHasDomainEvents)revision).DomainEvents.Single());
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Voided));
        Assert.That(entry.Summary, Is.EqualTo("Скасовано. Причина: Помилковий прихід"));
    }
}
