using ActivityLog.Contracts.Activity;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Activity.EventHandlers;

/// <summary>How an edit, void or restore of a fabric movement reads in «Журнал дій» (ACT-001).</summary>
public sealed class FabricRevisionActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent) =>
        domainEvent is FabricMovementRevised e ? Describe(e.Revision) : null;

    private static ActivityDescription Describe(FabricMovementRevision revision)
    {
        var (action, summary) = revision.Action switch
        {
            FabricMovementRevisionAction.Void => (ActivityActions.Voided, "Скасовано"),
            FabricMovementRevisionAction.Restore => (ActivityActions.Restored, "Повернуто стан до останньої правки"),
            _ => (ActivityActions.Changed, (string?)null),
        };
        if (revision.Reason is { } reason) summary = summary is null ? "Причина: " + reason : summary + ". Причина: " + reason;
        var before = revision.Before;
        var after = revision.After;
        var changes = revision.Action == FabricMovementRevisionAction.Edit
            ? new ActivityChanges()
                .Add("Дата", before.OperationDate, after.OperationDate)
                .Add("Метри", FabricMovementActivity.Meters(before.Delta), FabricMovementActivity.Meters(after.Delta))
                .Add("Постачальник", before.SupplierName, after.SupplierName)
                .Add("Документ", before.Document, after.Document)
                .Add("Куди", before.Destination, after.Destination)
                .Add("Позиція складу", before.StockId == after.StockId ? null : "попередня",
                    before.StockId == after.StockId ? null : FabricMovementActivity.Line(revision.Movement))
                .Items
            : [];
        return new ActivityDescription(action, FabricMovementActivity.Subject, FabricMovementActivity.SubjectTitle,
            revision.Movement.Id.ToString(CultureInfo.InvariantCulture), FabricMovementActivity.Line(revision.Movement),
            summary, changes, "Warehouse");
    }
}
