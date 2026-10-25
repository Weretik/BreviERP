namespace CatalogImport.Persistence;

public sealed class CatalogImportBatch
{
    public string SnapshotHash { get; set; } = string.Empty;
    public string SpreadsheetId { get; set; } = string.Empty;
    public DateTimeOffset CompletedAt { get; set; }
    public string SourceJson { get; set; } = string.Empty;
    public string ReportJson { get; set; } = string.Empty;
}

