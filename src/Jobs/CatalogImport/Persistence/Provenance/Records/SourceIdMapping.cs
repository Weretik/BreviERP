namespace CatalogImport.Persistence;

public sealed class SourceIdMapping
{
    public string Dataset { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public long TargetId { get; set; }
    public string SnapshotHash { get; set; } = string.Empty;
}

