namespace Warehouse.Domain.Stock.Movements;

/// <summary>
/// A form submission of «Склад тканин» by its operation id. The same id with the same data returns the saved result
/// instead of moving the fabric again; the same id with other data is refused.
/// </summary>
public sealed class StockOperation
{
    public const int KindMaxLength = 32;
    public const int HashLength = 64;

    private StockOperation() { }

    public StockOperation(Guid id, string kind, string requestHash, int userId, string fabricCode, string colorKey,
        string color, decimal metersAfter, DateTimeOffset now)
    {
        if (id == Guid.Empty) throw new ArgumentException("Operation id is required.", nameof(id));
        Id = id;
        Kind = kind;
        RequestHash = requestHash;
        UserId = userId;
        FabricCode = fabricCode;
        ColorKey = colorKey;
        Color = color;
        MetersAfter = metersAfter;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }
    public string Kind { get; private set; } = null!;

    /// <summary>SHA-256 of the person and the request without its version guard.</summary>
    public string RequestHash { get; private set; } = null!;

    public int UserId { get; private set; }
    public string FabricCode { get; private set; } = null!;
    public string ColorKey { get; private set; } = null!;
    public string Color { get; private set; } = null!;

    /// <summary>Metres of the pair right after the operation.</summary>
    public decimal MetersAfter { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
