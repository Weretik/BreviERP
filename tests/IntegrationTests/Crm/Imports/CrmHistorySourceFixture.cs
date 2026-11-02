using System.Security.Cryptography;
using System.Text.Json;

namespace IntegrationTests.Crm.Imports;

internal sealed class CrmHistorySourceFixture : IDisposable
{
    internal static readonly string[] Entities = ["counterparties", "quotes", "quote_versions", "quote_items", "quote_change_log", "quote_transfers", "quote_email_sends", "quote_ppe_price_overrides", "custom_sewing_drafts", "custom_sewing_versions", "custom_sewing_files", "introduction_saved_templates"];
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "brevi_history_source_" + Guid.NewGuid().ToString("N"));
    public string FreezePath => Path.Combine(Root, "freeze.json");
    public Dictionary<string, List<object>> Rows { get; } = Entities.ToDictionary(x => x, _ => new List<object>());
    public Dictionary<string, object?> Freeze { get; private set; } = new();
    public CrmHistorySourceFixture() => Directory.CreateDirectory(Root);

    public void Write(bool complete = true, string? optionsJson = null, Dictionary<string, int>? expectedCounts = null)
    {
        var data = Rows.ToDictionary(x => x.Key, x => (object)new { columns = Array.Empty<string>(), rows = x.Value });
        data["gmail_connections"] = new { columns = new[] { "token" }, rows = new[] { new { token = "PRIVATE_TOKEN" } } };
        data["transactions"] = new { columns = new[] { "bank" }, rows = new[] { new { bank = "PRIVATE_FINANCE" } } };
        File.WriteAllText(Path.Combine(Root, "database.json"), JsonSerializer.Serialize(data));
        var tables = Rows.ToDictionary(x => x.Key, x => new
        {
            status = complete ? "all_available_rows_read" : "partial",
            rows_exported = expectedCounts?.GetValueOrDefault(x.Key, x.Value.Count) ?? x.Value.Count,
            pages = 1,
            reached_last_page = complete,
            truncated_pages = complete ? Array.Empty<object>() : new object[] { new { truncated = true } },
            redacted_fields = Array.Empty<string>(),
            issues = Array.Empty<string>(),
            columns_match_source_schema = true
        });
        File.WriteAllText(Path.Combine(Root, "manifest.json"), JsonSerializer.Serialize(new { full_backup = complete, transaction_consistent_snapshot = complete, tables }));
        Freeze = new()
        {
            ["schemaVersion"] = 1,
            ["datasetId"] = "synthetic_history_v1",
            ["databaseFile"] = "database.json",
            ["databaseSha256"] = HashFile("database.json"),
            ["manifestFile"] = "manifest.json",
            ["manifestSha256"] = HashFile("manifest.json")
        };
        if (optionsJson is not null)
        {
            File.WriteAllText(Path.Combine(Root, "options.json"), optionsJson);
            Freeze["optionsFile"] = "options.json"; Freeze["optionsSha256"] = HashFile("options.json");
        }
        SaveFreeze();
    }
    public string HashFile(string relative) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, relative)))).ToLowerInvariant();
    public void SaveFreeze() => File.WriteAllText(FreezePath, JsonSerializer.Serialize(Freeze));
    public void Dispose() { if (Directory.Exists(Root) && Path.GetFileName(Root).StartsWith("brevi_history_source_", StringComparison.Ordinal)) Directory.Delete(Root, true); }
}
