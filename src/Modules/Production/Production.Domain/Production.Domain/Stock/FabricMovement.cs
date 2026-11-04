namespace Production.Domain.Stock;

/// <summary>Why the balance of a stock line changed.</summary>
public enum FabricMovementKind
{
    /// <summary>«Початковий залишок»: the first count of a line.</summary>
    Opening = 1,

    /// <summary>«Прихід» from a supplier.</summary>
    Receipt = 2,

    /// <summary>«Видача» to an item of an order.</summary>
    Issue = 3,

    /// <summary>«Коригування»: the counted amount differs from the balance.</summary>
    Adjustment = 4,

    /// <summary>«Сторно»: takes back another movement.</summary>
    Reversal = 5
}

/// <summary>What a movement says besides its amount.</summary>
public sealed record FabricMovementDetails(
    DateOnly OperationDate,
    string? Note,
    int? SupplierId = null,
    string? SupplierName = null,
    string? Document = null,
    FabricMovementTarget? Target = null);

/// <summary>The order item a movement went to.</summary>
public sealed record FabricMovementTarget(int OrderId, int ItemId, int OrderNumber, string ModelName);

/// <summary>
/// One change of a stock line: before, change and after in metres. It is never edited; a wrong one is taken back by a
/// «Сторно» movement that points to it.
/// </summary>
public sealed class FabricMovement
{
    public const int NoteMaxLength = 1000;
    public const int DocumentMaxLength = 200;
    public const int SupplierMaxLength = 300;
    public const int LegacyIdMaxLength = 64;

    private FabricMovement() { }

    internal FabricMovement(FabricStock stock, FabricMovementKind kind, decimal before, decimal delta,
        FabricMovementDetails details, string actorName, DateTimeOffset now, int? reversesMovementId, string? legacyId)
    {
        Stock = stock;
        Kind = kind;
        MetersBefore = before;
        Delta = delta;
        MetersAfter = before + delta;
        OperationDate = details.OperationDate;
        Note = Optional(details.Note, NoteMaxLength);
        SupplierId = details.SupplierId;
        SupplierName = Optional(details.SupplierName, SupplierMaxLength);
        Document = Optional(details.Document, DocumentMaxLength);
        OrderId = details.Target?.OrderId;
        ItemId = details.Target?.ItemId;
        OrderNumber = details.Target?.OrderNumber;
        ModelName = details.Target?.ModelName;
        ReversesMovementId = reversesMovementId;
        CreatedByName = ProductionText.Clean(actorName, 200);
        CreatedAt = now;
        LegacyId = Optional(legacyId, LegacyIdMaxLength);
    }

    public int Id { get; private set; }
    public int StockId { get; private set; }
    public FabricStock Stock { get; private set; } = null!;
    public FabricMovementKind Kind { get; private set; }
    public decimal MetersBefore { get; private set; }
    public decimal Delta { get; private set; }
    public decimal MetersAfter { get; private set; }

    /// <summary>The day the fabric really came or went; may be earlier than the record.</summary>
    public DateOnly OperationDate { get; private set; }

    public string? Note { get; private set; }

    /// <summary>«Постачальники» (Reference) of a receipt; null for the old app's typed names.</summary>
    public int? SupplierId { get; private set; }

    public string? SupplierName { get; private set; }

    /// <summary>Invoice or delivery note of a receipt.</summary>
    public string? Document { get; private set; }

    public int? OrderId { get; private set; }
    public int? ItemId { get; private set; }
    public int? OrderNumber { get; private set; }
    public string? ModelName { get; private set; }

    /// <summary>The movement a «Сторно» takes back.</summary>
    public int? ReversesMovementId { get; private set; }

    /// <summary>Taken back by a «Сторно»; it cannot be taken back twice.</summary>
    public bool IsReversed { get; private set; }

    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Id of the movement in the old «Цех»; makes a repeated transfer skip it.</summary>
    public string? LegacyId { get; private set; }

    internal void MarkReversed() => IsReversed = true;

    private static string? Optional(string? value, int maxLength)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        return text.Length > maxLength ? throw new ArgumentException($"Value is longer than {maxLength}.") : text;
    }
}
