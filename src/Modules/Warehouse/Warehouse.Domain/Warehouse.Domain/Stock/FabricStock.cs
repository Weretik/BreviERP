namespace Warehouse.Domain.Stock;

/// <summary>The fabric of «Тканини» a stock line holds, copied when the line is created.</summary>
public sealed record StockFabric(int? FabricId, string Code, string Name);

/// <summary>
/// «Склад тканин»: metres of one fabric (by FAB- code) in one colour. The balance changes only through
/// <see cref="FabricMovement"/>s; <see cref="Version"/> stops two people from moving it at once.
/// </summary>
public sealed class FabricStock
{
    public const int CodeMaxLength = 32;
    public const int NameMaxLength = 300;
    public const int LegacyIdMaxLength = 64;

    /// <summary>More than any shelf holds; guards against a typo.</summary>
    public const decimal MaxMeters = 1_000_000m;

    public const int Decimals = 2;

    private readonly List<FabricMovement> _movements = [];

    private FabricStock() { }

    public FabricStock(StockFabric fabric, string color, DateTimeOffset now, string? legacyId = null)
    {
        FabricId = fabric.FabricId;
        FabricCode = WarehouseText.Clean(fabric.Code, CodeMaxLength).ToUpperInvariant();
        FabricName = WarehouseText.Clean(fabric.Name, NameMaxLength);
        Color = StockColor.Clean(color);
        ColorKey = StockColor.Key(Color);
        CreatedAt = now;
        UpdatedAt = now;
        LegacyId = string.IsNullOrWhiteSpace(legacyId) ? null : legacyId.Trim();
    }

    public int Id { get; private set; }
    public int? FabricId { get; private set; }
    public string FabricCode { get; private set; } = null!;
    public string FabricName { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public string ColorKey { get; private set; } = null!;

    /// <summary>Balance in metres; may be negative (PRD-D27).</summary>
    public decimal Meters { get; private set; }

    public bool HasMovements { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; } = 1;
    public string? LegacyId { get; private set; }

    /// <summary>Movements added in this change; saved ones are read by a query.</summary>
    public IReadOnlyList<FabricMovement> Movements => _movements;

    public static bool IsValidMeters(decimal value) =>
        Math.Abs(value) <= MaxMeters && decimal.Round(value, Decimals) == value;

    /// <summary>Takes the fabric's current name from «Тканини».</summary>
    public void Rename(StockFabric fabric)
    {
        FabricId = fabric.FabricId ?? FabricId;
        FabricName = WarehouseText.Clean(fabric.Name, NameMaxLength);
    }

    /// <summary>
    /// «Початковий залишок» (the first movement, zero allowed), «Прихід» (positive), «Видача» (negative, may leave the
    /// balance below zero) or «Уточнення» (any non-zero change).
    /// </summary>
    public FabricMovement Move(FabricMovementKind kind, decimal delta, FabricMovementDetails details, string actorName,
        DateTimeOffset now)
    {
        if (kind is FabricMovementKind.Reversal) throw new ArgumentException("Use Reverse.", nameof(kind));
        if (kind == FabricMovementKind.Opening && (HasMovements || delta < 0))
            throw new InvalidOperationException("Opening balance is only the first movement.");
        if (kind == FabricMovementKind.Receipt && delta <= 0 || kind == FabricMovementKind.Issue && delta >= 0 ||
            kind == FabricMovementKind.Adjustment && delta == 0)
            throw new ArgumentOutOfRangeException(nameof(delta));
        return Add(kind, delta, details, actorName, now, null, null);
    }

    /// <summary>
    /// Recomputes before and after of every movement of this line in the order of operation date, record time and id
    /// (unsaved last), and the balance as the last «after».
    /// </summary>
    public void Restate(IEnumerable<FabricMovement> movements)
    {
        var chain = movements.Where(x => x.StockId == Id || ReferenceEquals(x.Stock, this)).Distinct()
            .OrderBy(x => x.OperationDate).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id == 0 ? int.MaxValue : x.Id)
            .ToList();
        var balance = 0m;
        foreach (var movement in chain)
        {
            movement.Chain(balance);
            balance = movement.MetersAfter;
        }

        if (balance == Meters) return;
        Meters = balance;
        Version++;
    }

    /// <summary>«Сторно»: the opposite change of a movement of this line, which is marked as taken back.</summary>
    public FabricMovement Reverse(FabricMovement original, FabricMovementDetails details, string actorName,
        DateTimeOffset now)
    {
        if (original.StockId != Id || original.Kind == FabricMovementKind.Reversal || original.IsReversed)
            throw new InvalidOperationException("This movement cannot be reversed.");
        original.MarkReversed();
        return Add(FabricMovementKind.Reversal, -original.Delta, details, actorName, now, original.Id, null);
    }

    /// <summary>
    /// Puts <paramref name="movement"/> on this line with a new state and records the revision. The movement keeps its
    /// kind and its sign: a receipt and an opening stay positive (an opening may be zero), an issue negative, an
    /// adjustment non-zero; a voided movement has no effect. The caller restates the lines it left and joined.
    /// </summary>
    public FabricMovementRevision Revise(FabricMovement movement, FabricMovementState state,
        FabricMovementRevisionAction action, string? reason, string actorName, DateTimeOffset now, Guid operationId,
        FabricMovementRevision? restores = null)
    {
        if (!movement.CanRevise) throw new InvalidOperationException("This movement cannot be revised here.");
        if (!IsValidMeters(state.Delta)) throw new ArgumentOutOfRangeException(nameof(state));
        var signed = state.IsVoided ? state.Delta == 0 : movement.Kind switch
        {
            FabricMovementKind.Opening => state.Delta >= 0,
            FabricMovementKind.Receipt => state.Delta > 0,
            FabricMovementKind.Issue => state.Delta < 0,
            _ => state.Delta != 0
        };
        if (!signed) throw new ArgumentOutOfRangeException(nameof(state));
        HasMovements = true;
        UpdatedAt = now;
        return movement.Revise(this, state, action, reason, actorName, now, operationId, restores);
    }

    /// <summary>A movement of the old «Цех» exactly as it was; its «before» must be the current balance.</summary>
    public FabricMovement Transfer(FabricMovementKind kind, decimal before, decimal delta,
        FabricMovementDetails details, string actorName, DateTimeOffset at, string legacyId)
    {
        if (before != Meters) throw new InvalidOperationException("The movement does not follow the balance.");
        return Add(kind, delta, details, actorName, at, null, legacyId);
    }

    private FabricMovement Add(FabricMovementKind kind, decimal delta, FabricMovementDetails details,
        string actorName, DateTimeOffset now, int? reverses, string? legacyId)
    {
        if (!IsValidMeters(delta) || !IsValidMeters(Meters + delta)) throw new ArgumentOutOfRangeException(nameof(delta));
        var movement = new FabricMovement(this, kind, Meters, delta, details, actorName, now, reverses, legacyId);
        _movements.Add(movement);
        Meters += delta;
        HasMovements = true;
        UpdatedAt = now;
        Version++;
        return movement;
    }
}
