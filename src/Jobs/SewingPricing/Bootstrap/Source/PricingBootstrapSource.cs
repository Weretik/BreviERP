using System.Security.Cryptography;
using System.Text.Json;
using Catalog.Domain.Pricing.Sewing;

namespace SewingPricing.Jobs.Bootstrap;

public sealed class PricingBootstrapSource
{
    public int Version { get; set; }
    public string SpreadsheetId { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; }
    public string CatalogSnapshotHash { get; set; } = string.Empty;
    public JsonElement[][] Settings { get; set; } = [];
    public Dictionary<string, string> Formulas { get; set; } = [];

    public static async Task<(PricingBootstrapSource Source, string Hash, string Json)> LoadAsync(
        string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var source = JsonSerializer.Deserialize<PricingBootstrapSource>(bytes, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Empty pricing source.");
        if (source.Version != 1 || source.CatalogSnapshotHash.Length != 64 || source.Formulas.Count == 0
            || source.CapturedAt == default || string.IsNullOrWhiteSpace(source.SpreadsheetId))
            throw new InvalidDataException("Complete frozen pricing provenance is required.");
        return (source, Convert.ToHexString(SHA256.HashData(bytes)), System.Text.Encoding.UTF8.GetString(bytes));
    }
}

