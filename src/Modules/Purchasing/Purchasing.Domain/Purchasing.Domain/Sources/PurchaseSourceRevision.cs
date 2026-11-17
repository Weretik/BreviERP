using BuildingBlocks.Domain.Entity;
namespace Purchasing.Domain.Sources;

/// <summary>An immutable version of a source row's raw cells; the same raw text never makes a second revision.</summary>
public sealed class PurchaseSourceRevision : IAggregateRoot
{
    private PurchaseSourceRevision() { }

    private PurchaseSourceRevision(string id, string sourceId, string rawJson, DateTimeOffset at)
    {
        Id = id;
        SourceId = sourceId;
        RawJson = rawJson;
        At = at;
    }

    public static PurchaseSourceRevision Create(string id, string sourceId, string rawJson, DateTimeOffset at) =>
        new(id, sourceId, rawJson, at);

    /// <summary>SHA-256 of sourceId:rawJson, lower-case hex.</summary>
    public string Id { get; private set; } = null!;

    public string SourceId { get; private set; } = null!;
    public string RawJson { get; private set; } = null!;
    public DateTimeOffset At { get; private set; }
}
