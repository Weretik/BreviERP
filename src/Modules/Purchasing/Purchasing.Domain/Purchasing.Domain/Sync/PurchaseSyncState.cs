using BuildingBlocks.Domain.Entity;
namespace Purchasing.Domain.Sync;

/// <summary>Result of the last sheet sync: its time, counts and the last error.</summary>
public sealed class PurchaseSyncState : IAggregateRoot
{
    public const string GoogleFinance = "google-finance";
    public const int ErrorMaxLength = 1_000;

    private PurchaseSyncState() { }

    private PurchaseSyncState(string id) => Id = id;

    public static PurchaseSyncState Create(string id) =>
        new(id);

    public string Id { get; private set; } = null!;
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public DateTimeOffset? LastSuccessAt { get; private set; }
    public string? Error { get; private set; }

    /// <summary>Active source rows of the last successful sync, not messages.</summary>
    public int RecordCount { get; private set; }

    /// <summary>Active file links of the last successful sync.</summary>
    public int FileCount { get; private set; }

    public void Succeeded(DateTimeOffset at, int recordCount, int fileCount)
    {
        LastAttemptAt = at;
        LastSuccessAt = at;
        Error = null;
        RecordCount = recordCount;
        FileCount = fileCount;
    }

    public void Failed(DateTimeOffset at, string error)
    {
        LastAttemptAt = at;
        Error = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
    }
}
