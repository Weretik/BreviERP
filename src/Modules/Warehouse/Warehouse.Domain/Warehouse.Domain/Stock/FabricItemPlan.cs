namespace Warehouse.Domain.Stock;

/// <summary>Role of a fabric in a model (TZ §5.2); each layer has its own plan.</summary>
public enum FabricLayer
{
    /// <summary>«Основна».</summary>
    Upper = 1,

    /// <summary>«Підкладка».</summary>
    Lining = 2,

    /// <summary>«Утеплювач».</summary>
    Insulation = 3,

    /// <summary>«Фліс».</summary>
    Fleece = 4
}

/// <summary>A planned part of a saved item plan: fabric, colour, layer and metres for the whole quantity.</summary>
public sealed class FabricItemPlanPart
{
    private FabricItemPlanPart() { }

    public FabricItemPlanPart(string fabricCode, string color, string colorKey, FabricLayer layer, decimal meters)
    {
        FabricCode = fabricCode;
        Color = color;
        ColorKey = colorKey;
        Layer = layer;
        Meters = meters;
    }

    public string FabricCode { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public string ColorKey { get; private set; } = null!;
    public FabricLayer Layer { get; private set; }
    public decimal Meters { get; private set; }

    public FabricPlanKey Key => new(FabricCode, ColorKey);
}

/// <summary>
/// The fabric plan of an order item as it was when fabric was first issued (TZ §5.2): issues compare with it, not with
/// today's norm of the model. «Оновити план» replaces it explicitly and raises the version; issues already made keep
/// the plan they were compared with.
/// </summary>
public sealed class FabricItemPlan
{
    private readonly List<FabricItemPlanPart> _parts = [];

    private FabricItemPlan() { }

    public FabricItemPlan(int orderId, int itemId, int quantity, IEnumerable<FabricItemPlanPart> parts,
        string actorName, DateTimeOffset now)
    {
        OrderId = orderId;
        ItemId = itemId;
        Replace(quantity, parts, actorName, now);
        Version = 1;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }

    /// <summary>One plan per item; the item is Production's, kept by value.</summary>
    public int ItemId { get; private set; }

    /// <summary>Number of products the plan is for.</summary>
    public int Quantity { get; private set; }

    /// <summary>From 1; an issue sends the version it was prepared with.</summary>
    public int Version { get; private set; }

    public string UpdatedByName { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyList<FabricItemPlanPart> Parts => _parts;

    /// <summary>«Оновити план»: the current norm replaces the saved plan.</summary>
    public void Refresh(int quantity, IEnumerable<FabricItemPlanPart> parts, string actorName, DateTimeOffset now)
    {
        Replace(quantity, parts, actorName, now);
        Version++;
    }

    private void Replace(int quantity, IEnumerable<FabricItemPlanPart> parts, string actorName, DateTimeOffset now)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Quantity = quantity;
        _parts.Clear();
        _parts.AddRange(parts);
        UpdatedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        UpdatedAt = now;
    }

    /// <summary>
    /// The plan of <paramref name="cut"/> products out of the plan for the whole quantity: P × n / N in hundredths,
    /// half up (TZ §7.2). 30 m for 10 products, 4 cut → 12 m.
    /// </summary>
    public static decimal ComparisonPlan(decimal meters, int cut, int quantity)
    {
        if (quantity <= 0 || cut <= 0) return 0m;
        var hundredths = (long)decimal.Round(meters * 100m);
        var value = (hundredths * cut * 2 + quantity) / (2L * quantity);
        return value / 100m;
    }
}
