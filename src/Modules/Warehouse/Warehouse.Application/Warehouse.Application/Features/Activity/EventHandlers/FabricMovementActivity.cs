using ActivityLog.Contracts.Activity;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Activity.EventHandlers;

/// <summary>How a new fabric movement reads in «Журнал дій» (ACT-001): kind, metres, supplier or order.</summary>
public sealed class FabricMovementActivity : IActivityDescriber
{
    public const string Subject = "fabric-movement";
    public const string SubjectTitle = "Рух тканини";

    public ActivityDescription? Describe(object domainEvent) =>
        domainEvent is FabricMovementRecorded e ? Describe(e.Movement) : null;

    private static ActivityDescription Describe(FabricMovement movement)
    {
        var parts = new List<string> { Kind(movement.Kind) + ": " + Meters(movement.Delta) };
        if (movement.OrderNumber is { } order) parts.Add($"на № {order} {movement.ModelName}".TrimEnd());
        if (movement.SupplierName is { } supplier) parts.Add(supplier);
        if (movement.Document is { } document) parts.Add("документ " + document);
        if (movement.Destination is { } destination) parts.Add(destination);
        if (movement.Note is { } note) parts.Add(note);
        return new ActivityDescription(ActivityActions.Created, Subject, SubjectTitle,
            movement.Id.Value.ToString(CultureInfo.InvariantCulture), Line(movement), string.Join("; ", parts), [], "Warehouse");
    }

    /// <summary>«TK-12 Грета · Синій»: the stock line of the movement.</summary>
    public static string Line(FabricMovement movement) =>
        $"{movement.Stock.FabricCode} {movement.Stock.FabricName} · {movement.Stock.Color}";

    /// <summary>«+12,5 м» or «−3 м».</summary>
    public static string Meters(decimal delta) =>
        (delta > 0 ? "+" : delta < 0 ? "−" : "") + ActivityChanges.Number(Math.Abs(delta), "м");

    private static string Kind(FabricMovementKind kind) => kind switch
    {
        FabricMovementKind.Opening => "Початковий залишок",
        FabricMovementKind.Receipt => "Прихід",
        FabricMovementKind.Issue => "Видача",
        FabricMovementKind.Adjustment => "Коригування",
        _ => "Сторно",
    };
}
