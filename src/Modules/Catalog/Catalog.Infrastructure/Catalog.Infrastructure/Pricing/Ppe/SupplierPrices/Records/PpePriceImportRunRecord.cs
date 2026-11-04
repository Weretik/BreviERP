namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Records;

/// <summary>One finished supplier price import with its counts and problems (JSON).</summary>
public sealed class PpePriceImportRunRecord
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset FinishedAt { get; set; }
    public int Checked { get; set; }
    public int Changed { get; set; }
    public int Unchanged { get; set; }
    public int Skipped { get; set; }
    public string ProblemsJson { get; set; } = "[]";
}
