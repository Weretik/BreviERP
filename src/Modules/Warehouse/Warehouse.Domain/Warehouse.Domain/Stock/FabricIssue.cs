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
/// «Видача»: fabric handed to the workshop for one item, as one package of lines. Each line takes metres from one
/// stock line by its movement. The package is reversed whole («Сторно видачі»).
/// </summary>
public sealed class FabricIssue
{
    public const int NoteMaxLength = 1000;
    public const int MaxLines = 20;

    private readonly List<FabricIssueLine> _lines = [];

    private FabricIssue() { }

    public FabricIssue(FabricMovementTarget target, DateOnly issuedOn, string? note, string actorName,
        DateTimeOffset now, string? legacyId = null)
    {
        OrderId = target.OrderId;
        ItemId = target.ItemId;
        OrderNumber = target.OrderNumber;
        ModelName = target.ModelName;
        IssuedOn = issuedOn;
        Note = string.IsNullOrWhiteSpace(note) ? null : WarehouseText.Clean(note, NoteMaxLength);
        CreatedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        CreatedAt = now;
        LegacyId = string.IsNullOrWhiteSpace(legacyId) ? null : legacyId.Trim();
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
    public IReadOnlyList<FabricIssueLine> Lines => _lines;
    public bool IsReversed => ReversedAt is not null;

    public FabricMovementTarget Target => new(OrderId, ItemId, OrderNumber, ModelName);

    public void AddLine(FabricIssuePurpose purpose, FabricPlanKey? plan, decimal? plannedMeters, FabricStock stock,
        FabricMovement movement, decimal covered)
    {
        if (_lines.Count >= MaxLines) throw new InvalidOperationException("Too many lines.");
        if (purpose == FabricIssuePurpose.Additional ? plan is not null || covered != 0 : plan is null)
            throw new ArgumentException("A planned or replacement line needs its plan part; an additional one has none.");
        if (covered < 0) throw new ArgumentOutOfRangeException(nameof(covered));
        _lines.Add(new FabricIssueLine(purpose, plan, plannedMeters, stock, movement, covered));
    }

    public void Reverse(string reason, string actorName, DateTimeOffset now)
    {
        if (IsReversed) throw new InvalidOperationException("Already reversed.");
        ReversedAt = now;
        ReversedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        ReverseReason = WarehouseText.Clean(reason, NoteMaxLength);
    }
}

/// <summary>One fabric and colour of a «Видача».</summary>
public sealed class FabricIssueLine
{
    private FabricIssueLine() { }

    internal FabricIssueLine(FabricIssuePurpose purpose, FabricPlanKey? plan, decimal? plannedMeters,
        FabricStock stock, FabricMovement movement, decimal covered)
    {
        Purpose = purpose;
        PlanFabricCode = plan?.FabricCode;
        PlanColorKey = plan?.ColorKey;
        PlannedMeters = plannedMeters;
        Stock = stock;
        FabricCode = stock.FabricCode;
        FabricName = stock.FabricName;
        Color = stock.Color;
        Movement = movement;
        Meters = -movement.Delta;
        CoveredMeters = covered;
    }

    public int Id { get; private set; }
    public int IssueId { get; private set; }
    public FabricIssuePurpose Purpose { get; private set; }
    public string? PlanFabricCode { get; private set; }
    public string? PlanColorKey { get; private set; }

    /// <summary>The planned part at the moment of the issue, for the comparison «план / факт».</summary>
    public decimal? PlannedMeters { get; private set; }

    public int StockId { get; private set; }
    public FabricStock Stock { get; private set; } = null!;
    public string FabricCode { get; private set; } = null!;
    public string FabricName { get; private set; } = null!;
    public string Color { get; private set; } = null!;

    /// <summary>Issued metres (positive).</summary>
    public decimal Meters { get; private set; }

    /// <summary>How much of the planned part this line covers.</summary>
    public decimal CoveredMeters { get; private set; }

    public int MovementId { get; private set; }
    public FabricMovement Movement { get; private set; } = null!;

    public FabricPlanKey? Plan => PlanFabricCode is null || PlanColorKey is null ? null : new(PlanFabricCode, PlanColorKey);
}
