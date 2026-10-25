using System.Security.Cryptography;
using System.Text.Json;
using CatalogImport.Source;
namespace CatalogImport.PpePricing.Source;
public sealed class PpePricingSource
{
    public int Version { get; set; }
    public string SpreadsheetId { get; set; } = string.Empty;
    public string CapturedAt { get; set; } = string.Empty;
    public JsonElement[][] Rules { get; set; } = [];
    public JsonElement[][] Products { get; set; } = [];
    public IEnumerable<WorkbookRow> RuleRows() => Rules.Skip(1).Select((r,i)=>new WorkbookRow("Налаштування цін",i+2,r)).Where(x=>x.Text(0).Length>0);
    public IEnumerable<WorkbookRow> ProductRows() => Products.Skip(1).Select((r,i)=>new WorkbookRow("ЗІЗ",i+2,r)).Where(x=>x.Text(0).Length>0);
    public static async Task<(PpePricingSource Source, string Hash)> LoadAsync(string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path,cancellationToken);
        var source = JsonSerializer.Deserialize<PpePricingSource>(System.Text.Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'),new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (source is null || source.Version!=1 || string.IsNullOrWhiteSpace(source.SpreadsheetId)
            || !DateOnly.TryParse(source.CapturedAt, out _) || !source.RuleRows().Any() || !source.ProductRows().Any())
            throw new InvalidDataException("Complete version-1 PPE source and provenance are required.");
        return (source,Convert.ToHexString(SHA256.HashData(bytes)));
    }
}
