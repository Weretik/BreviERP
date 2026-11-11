namespace Warehouse.Domain.Stock;

/// <summary>What a line of a «Видача» is for (PRD-D30).</summary>
public enum FabricIssuePurpose
{
    /// <summary>«Планова»: the planned fabric and colour; covers the plan up to what was issued.</summary>
    Planned = 1,

    /// <summary>«Заміна»: another fabric or colour instead of a planned part; closes that part.</summary>
    Replacement = 2,

    /// <summary>«Додаткова»: beyond the plan; covers nothing.</summary>
    Additional = 3
}

/// <summary>The planned part of an item a line covers: fabric code and colour key, as in the plan.</summary>
public sealed record FabricPlanKey(string FabricCode, string ColorKey);

/// <summary>
/// «Видача»: fabric handed to the workshop for one item, as one package of lines confirmed at once. An issue by actual
/// cutting (TZ §7) says how many products were cut; each line is a fabric and colour pair whose metres may come from
/// several stock lines of the pair, one movement each. The package is reversed whole («Сторно видачі»).
/// </summary>
public sealed class FabricIssue
{
    public const int NoteMaxLength = 1000;
    public const int MaxLines = 20;

    private readonly List<FabricIssueLine> _lines = [];

    private FabricIssue() { }

    public FabricIssue(FabricMovementTarget target, DateOnly issuedOn, string? note, string actorName,
        DateTimeOffset now, string? legacyId = null, int? issuedQuantity = null, Guid? operationId = null,
        int? planVersion = null)
    {
        if (issuedQuantity is <= 0) throw new ArgumentOutOfRangeException(nameof(issuedQuantity));
        OrderId = target.OrderId;
        ItemId = target.ItemId;
        OrderNumber = target.OrderNumber;
        ModelName = target.ModelName;
        IssuedOn = issuedOn;
        Note = string.IsNullOrWhiteSpace(note) ? null : WarehouseText.Clean(note, NoteMaxLength);
        CreatedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        CreatedAt = now;
        LegacyId = string.IsNullOrWhiteSpace(legacyId) ? null : legacyId.Trim();
        IssuedQuantity = issuedQuantity;
        OperationId = operationId;
        PlanVersion = planVersion;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ItemId { get; private set; }
    public int OrderNumber { get; private set; }
    public string ModelName { get; private set; } = null!;
    public DateOnly IssuedOn { get; private set; }
    public string? Note { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReversedAt { get; private set; }
    public string? ReversedByName { get; private set; }
    public string? ReverseReason { get; private set; }
    public string? LegacyId { get; private set; }

    /// <summary>Products cut for this issue; null for issues made before issues by actual cutting.</summary>
    public int? IssuedQuantity { get; private set; }

    /// <summary>The form submission; a repeat returns the saved issue.</summary>
    public Guid? OperationId { get; private set; }

    /// <summary>Version of the saved item plan the issue was compared with.</summary>
    public int? PlanVersion { get; private set; }

    public IReadOnlyList<FabricIssueLine> Lines => _lines;
    public bool IsReversed => ReversedAt is not null;

    public FabricMovementTarget Target => new(OrderId, ItemId, OrderNumber, ModelName);

    /// <param name="comparisonPlan">The plan of the products cut for the planned part; the same for every line of it.</param>
    /// <param name="covered">How much of the planned part this line covers.</param>
    /// <param name="layer">The layer of the planned part, or the role an additional line was given (null: unknown).</param>
    public FabricIssueLine AddLine(FabricIssuePurpose purpose, FabricPlanKey? plan, decimal? comparisonPlan,
        IReadOnlyList<FabricMovement> movements, decimal covered, FabricLayer? layer = null, string? note = null)
    {
        if (_lines.Count >= MaxLines) throw new InvalidOperationException("Too many lines.");
        if (purpose == FabricIssuePurpose.Additional ? plan is not null || covered != 0 : plan is null)
            throw new ArgumentException("A planned or replacement line needs its plan part; an additional one has none.");
        if (covered < 0) throw new ArgumentOutOfRangeException(nameof(covered));
        if (movements.Count == 0) throw new ArgumentException("A line needs at least one movement.", nameof(movements));
        var line = new FabricIssueLine(purpose, plan, comparisonPlan, movements, covered, layer, note);
        _lines.Add(line);
        return line;
    }

    public void Reverse(string reason, string actorName, DateTimeOffset now)
    {
        if (IsReversed) throw new InvalidOperationException("Already reversed.");
        ReversedAt = now;
        ReversedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        ReverseReason = WarehouseText.Clean(reason, NoteMaxLength);
    }
}

/// <summary>
/// One fabric and colour of a «Видача». Its metres are the sum of its movements (one per stock line of the pair it was
/// spent from), so an edit or void of a movement updates the line (TZ §8).
/// </summary>
public sealed class FabricIssueLine
{
    public const int NoteMaxLength = 500;

    private readonly List<FabricMovement> _movements = [];

    private FabricIssueLine() { }

    internal FabricIssueLine(FabricIssuePurpose purpose, FabricPlanKey? plan, decimal? comparisonPlan,
        IReadOnlyList<FabricMovement> movements, decimal covered, FabricLayer? layer, string? note)
    {
        Purpose = purpose;
        PlanFabricCode = plan?.FabricCode;
        PlanColorKey = plan?.ColorKey;
        PlannedMeters = comparisonPlan;
        _movements.AddRange(movements);
        PlanCoverage = covered;
        Layer = layer;
        Note = string.IsNullOrWhiteSpace(note) ? null : WarehouseText.Clean(note, NoteMaxLength);
        Sync(derivePurpose: false);
    }

    public int Id { get; private set; }
    public int IssueId { get; private set; }
    public FabricIssuePurpose Purpose { get; private set; }
    public string? PlanFabricCode { get; private set; }
    public string? PlanColorKey { get; private set; }

    /// <summary>The plan of the products cut for its planned part (issues by cutting), or the planned part then.</summary>
    public decimal? PlannedMeters { get; private set; }

    /// <summary>The first stock line the metres came from; the pair is its fabric group and colour.</summary>
    public int StockId { get; private set; }

    public FabricStock Stock { get; private set; } = null!;
    public string FabricCode { get; private set; } = null!;
    public string FabricName { get; private set; } = null!;
    public string Color { get; private set; } = null!;

    /// <summary>Issued metres (positive): the sum of its movements.</summary>
    public decimal Meters { get; private set; }

    /// <summary>The coverage the issue gave its planned part.</summary>
    public decimal PlanCoverage { get; private set; }

    /// <summary>How much of the planned part this line covers now: none while all its movements are voided.</summary>
    public decimal CoveredMeters { get; private set; }

    public FabricLayer? Layer { get; private set; }
    public string? Note { get; private set; }

    /// <summary>The movements of this line, one per stock line it was spent from.</summary>
    public IReadOnlyList<FabricMovement> Movements => _movements;

    public FabricPlanKey? Plan => PlanFabricCode is null || PlanColorKey is null ? null : new(PlanFabricCode, PlanColorKey);

    /// <summary>Every movement of the line is voided.</summary>
    public bool IsVoided => _movements.Count > 0 && _movements.All(x => x.IsVoided);

    /// <summary>
    /// After a movement of the line changed: metres, the stock line and the fabric shown, «планова» or «заміна» against
    /// its planned part, and the coverage (none while voided).
    /// </summary>
    public void Sync(bool derivePurpose = true)
    {
        var first = _movements.OrderBy(x => x.IsVoided).ThenByDescending(x => -x.Delta).First();
        Stock = first.Stock;
        StockId = first.Stock.Id;
        FabricCode = first.Stock.FabricCode;
        FabricName = first.Stock.FabricName;
        Color = first.Stock.Color;
        Meters = -_movements.Sum(x => x.Delta);
        if (derivePurpose && Plan is { } plan)
            Purpose = FabricAliases.Canonical(plan.FabricCode) == FabricAliases.Canonical(first.Stock.FabricCode) &&
                      plan.ColorKey == first.Stock.ColorKey
                ? FabricIssuePurpose.Planned
                : FabricIssuePurpose.Replacement;
        CoveredMeters = IsVoided ? 0m : PlanCoverage;
    }
}
