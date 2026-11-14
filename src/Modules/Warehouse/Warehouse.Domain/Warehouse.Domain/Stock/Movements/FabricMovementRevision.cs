namespace Warehouse.Domain.Stock.Movements;

/// <summary>What a revision of a movement did.</summary>
public enum FabricMovementRevisionAction
{
    /// <summary>Date, metres, supplier, document, destination or the fabric and colour changed.</summary>
    Edit = 1,

    /// <summary>«Скасувати»: the movement stays in the history with no effect on the balance.</summary>
    Void = 2,

    /// <summary>«Скасувати останню правку»: the state before the last edit or void came back.</summary>
    Restore = 3
}

/// <summary>The parts of a movement an edit, a void or a restore may change.</summary>
public sealed record FabricMovementState(int StockId, DateOnly OperationDate, decimal Delta, string? SupplierName,
    string? Document, string? Destination, bool IsVoided);

/// <summary>
/// One change of a movement: who, when, why and the values before and after. A movement is never edited without one,
/// so the history shows every edit, void and restore and the last one can be taken back.
/// </summary>
public sealed class FabricMovementRevision
{
    public const int ReasonMaxLength = 500;

    private FabricMovementRevision() { }

    internal FabricMovementRevision(FabricMovement movement, int number, FabricMovementRevisionAction action,
        FabricMovementState before, FabricStock afterStock, FabricMovementState after, string? reason, string actorName, DateTimeOffset now,
        Guid operationId, int? restoresRevisionId)
    {
        Movement = movement;
        Number = number;
        Action = action;
        BeforeStockId = before.StockId;
        BeforeOperationDate = before.OperationDate;
        BeforeDelta = before.Delta;
        BeforeSupplierName = before.SupplierName;
        BeforeDocument = before.Document;
        BeforeDestination = before.Destination;
        BeforeIsVoided = before.IsVoided;
        AfterStock = afterStock;
        AfterStockId = afterStock.Id;
        AfterOperationDate = after.OperationDate;
        AfterDelta = after.Delta;
        AfterSupplierName = after.SupplierName;
        AfterDocument = after.Document;
        AfterDestination = after.Destination;
        AfterIsVoided = after.IsVoided;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : WarehouseText.Clean(reason, ReasonMaxLength);
        CreatedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        CreatedAt = now;
        OperationId = operationId;
        RestoresRevisionId = restoresRevisionId;
    }

    public int Id { get; private set; }
    public int MovementId { get; private set; }
    public FabricMovement Movement { get; private set; } = null!;

    /// <summary>The movement's revision number after this change, from 1.</summary>
    public int Number { get; private set; }

    public FabricMovementRevisionAction Action { get; private set; }

    public int BeforeStockId { get; private set; }
    public DateOnly BeforeOperationDate { get; private set; }
    public decimal BeforeDelta { get; private set; }
    public string? BeforeSupplierName { get; private set; }
    public string? BeforeDocument { get; private set; }
    public string? BeforeDestination { get; private set; }
    public bool BeforeIsVoided { get; private set; }

    public int AfterStockId { get; private set; }

    /// <summary>The line the movement is on after the change; may be a line this change created.</summary>
    public FabricStock AfterStock { get; private set; } = null!;
    public DateOnly AfterOperationDate { get; private set; }
    public decimal AfterDelta { get; private set; }
    public string? AfterSupplierName { get; private set; }
    public string? AfterDocument { get; private set; }
    public string? AfterDestination { get; private set; }
    public bool AfterIsVoided { get; private set; }

    public string? Reason { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid OperationId { get; private set; }

    /// <summary>The edit or void a restore took back.</summary>
    public int? RestoresRevisionId { get; private set; }

    /// <summary>Taken back by a later restore.</summary>
    public bool IsUndone { get; private set; }

    public FabricMovementState Before => new(BeforeStockId, BeforeOperationDate, BeforeDelta, BeforeSupplierName,
        BeforeDocument, BeforeDestination, BeforeIsVoided);

    public FabricMovementState After => new(AfterStockId, AfterOperationDate, AfterDelta, AfterSupplierName,
        AfterDocument, AfterDestination, AfterIsVoided);

    internal void MarkUndone() => IsUndone = true;
}
