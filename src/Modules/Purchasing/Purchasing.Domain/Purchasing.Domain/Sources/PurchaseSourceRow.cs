namespace Purchasing.Domain.Sources;

/// <summary>
/// One row (UID) of the source sheet. The row keeps its id when it moves; an inactive row is a row that disappeared from
/// the sheet and is kept for history.
/// </summary>
public sealed class PurchaseSourceRow
{
    private PurchaseSourceRow() { }

    public PurchaseSourceRow(string id, string messageId, string sourceUid, int rowNumber, string rawJson,
        IReadOnlyList<string> issues, Guid run, DateTimeOffset now)
    {
        Id = id;
        MessageId = messageId;
        SourceUid = sourceUid;
        RowNumber = rowNumber;
        RawJson = rawJson;
        Issues = [.. issues];
        Active = true;
        SeenRun = run;
        UpdatedAt = now;
    }

    /// <summary>SHA-256 of spreadsheetId:sheetId:trim(UID), lower-case hex.</summary>
    public string Id { get; private set; } = null!;

    public string MessageId { get; private set; } = null!;

    /// <summary>The UID cell as a string; never converted to a number.</summary>
    public string SourceUid { get; private set; } = null!;

    /// <summary>Last known row in the sheet, from 2; a navigation hint, not an identity.</summary>
    public int RowNumber { get; private set; }

    /// <summary>The 14 named A:N cells serialised exactly like JavaScript JSON.stringify.</summary>
    public string RawJson { get; private set; } = null!;

    /// <summary>Reasons why the row needs a manual check; empty when nothing is wrong.</summary>
    public List<string> Issues { get; private set; } = [];

    public bool Active { get; private set; }
    public Guid SeenRun { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Refreshes the row from the sheet; returns true when its data changed (not only the run).</summary>
    public bool Apply(int rowNumber, string rawJson, IReadOnlyList<string> issues, Guid run, DateTimeOffset now)
    {
        var changed = !Active || RowNumber != rowNumber || RawJson != rawJson || !Issues.SequenceEqual(issues);
        SeenRun = run;
        if (!changed) return false;
        RowNumber = rowNumber;
        RawJson = rawJson;
        Issues = [.. issues];
        Active = true;
        UpdatedAt = now;
        return true;
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!Active) return;
        Active = false;
        UpdatedAt = now;
    }
}
