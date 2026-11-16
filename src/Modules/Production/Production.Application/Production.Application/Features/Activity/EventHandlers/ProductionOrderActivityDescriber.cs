using ActivityLog.Contracts.Activity;
using Production.Domain.Orders;
using Production.Domain.Orders.Events;

namespace Production.Application.Features.Activity.EventHandlers;

/// <summary>
/// How «Виробниче завдання» actions read in «Журнал дій» (ACT-001). The order's own «Історія змін» lines are already
/// display texts, so they become the changed fields as they are.
/// </summary>
public sealed class ProductionOrderActivityDescriber : IActivityDescriber
{
    public const string Subject = "production-order";

    public ActivityDescription? Describe(object domainEvent) =>
        domainEvent is ProductionOrderRecorded e ? Describe(e) : null;

    private static ActivityDescription? Describe(ProductionOrderRecorded e) => e.Kind switch
    {
        ProductionOrderActivity.Created => Of(e.Order, ActivityActions.Created, Origin(e.Order), []),
        ProductionOrderActivity.Changed or ProductionOrderActivity.StagesChanged =>
            Changes(e.Lines) is { Any: true } changes ? Of(e.Order, ActivityActions.Changed, null, changes.Items) : null,
        ProductionOrderActivity.Shipped => Of(e.Order, ActivityActions.Changed, Note(e.Lines.FirstOrDefault()),
            Changes(e.Lines.Skip(1)).Items),
        ProductionOrderActivity.ShipmentCancelled or ProductionOrderActivity.Noted =>
            Of(e.Order, ActivityActions.Changed, Note(e.Lines.FirstOrDefault()), []),
        ProductionOrderActivity.Closed => Of(e.Order, ActivityActions.Closed, null, []),
        ProductionOrderActivity.Cancelled => Of(e.Order, ActivityActions.Cancelled, e.Lines.FirstOrDefault()?.After, []),
        ProductionOrderActivity.Reopened => Of(e.Order, ActivityActions.Reopened, null, []),
        ProductionOrderActivity.Deleted => Of(e.Order, ActivityActions.Deleted, null, []),
        _ => null,
    };

    private static string? Origin(ProductionOrder order) =>
        order.QuoteNumber is { } quote ? "З КП " + quote : order.CopiedFromOrderId is null ? null : "Копія завдання";

    private static ActivityChanges Changes(IEnumerable<ProductionOrderChange> lines)
    {
        var changes = new ActivityChanges();
        foreach (var line in lines) changes.Add(Label(line), line.Before, line.After);
        return changes;
    }

    /// <summary>«Куртка: Файл додано: ескіз.pdf»: the line as one sentence.</summary>
    private static string? Note(ProductionOrderChange? line) =>
        line is null ? null : $"{Label(line)}: {line.After ?? line.Before}";

    private static string Label(ProductionOrderChange line) =>
        line.ItemName is null ? line.Field : $"{line.ItemName}: {line.Field}";

    private static ActivityDescription Of(ProductionOrder order, string action, string? summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, "Виробниче завдання", order.Id.ToString(CultureInfo.InvariantCulture),
            $"№ {order.Number.ToString(CultureInfo.InvariantCulture)} · {order.ClientName}", summary, changes,
            "Production");
}
