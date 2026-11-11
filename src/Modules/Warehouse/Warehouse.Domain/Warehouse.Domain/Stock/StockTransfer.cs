namespace Warehouse.Domain.Stock;

/// <summary>
/// The journal of «Перенесення з v133» (WH-006): one record per export that replaced the stock, known by the SHA-256
/// of its rows, with the counts it gave. A repeat of the same export finds it and changes nothing.
/// </summary>
public sealed class StockTransfer
{
    public const int HashLength = 64;
    public const int SnapshotMaxLength = 200;

    private StockTransfer() { }

    public StockTransfer(string hash, string? snapshot, int lines, int movements, int issues, decimal meters,
        string actorName, DateTimeOffset now)
    {
        if (hash.Length != HashLength) throw new ArgumentException("SHA-256 in hex is required.", nameof(hash));
        Hash = hash;
        Snapshot = string.IsNullOrWhiteSpace(snapshot) ? null : WarehouseText.Clean(snapshot, SnapshotMaxLength);
        Lines = lines;
        Movements = movements;
        Issues = issues;
        Meters = meters;
        CreatedByName = WarehouseText.Clean(actorName, FabricMovement.PersonMaxLength);
        CreatedAt = now;
    }

    public int Id { get; private set; }
    public string Hash { get; private set; } = null!;
    public string? Snapshot { get; private set; }
    public int Lines { get; private set; }
    public int Movements { get; private set; }
    public int Issues { get; private set; }
    public decimal Meters { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}
