namespace CatalogImport.Execution;

public sealed class CatalogImportReport
{
    public string SnapshotHash { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Products { get; set; }
    public int Articles { get; set; }
    public int Materials { get; set; }
    public int Operations { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<string> ReferenceChanges { get; set; } = [];
    public Dictionary<string, int> References { get; set; } = [];
}
