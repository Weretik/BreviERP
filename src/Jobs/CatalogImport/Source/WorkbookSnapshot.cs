using System.Security.Cryptography;
using System.Text.Json;

namespace CatalogImport.Source;

public sealed class WorkbookSnapshot
{
    public int Version { get; set; }
    public string SpreadsheetId { get; set; } = string.Empty;
    public string CapturedAt { get; set; } = string.Empty;
    public Dictionary<string, JsonElement[][]> Sheets { get; set; } = [];
    public Dictionary<string, string> ModelWebsiteUrls { get; set; } = [];

    public IReadOnlyList<WorkbookRow> Rows(string sheet, int keyColumn = 0)
    {
        if (!Sheets.TryGetValue(sheet, out var rows))
            throw new InvalidDataException($"Required sheet is absent: {sheet}.");
        return rows.Skip(1).Select((row, index) => new WorkbookRow(sheet, index + 2, row))
            .Where(row => row.Text(keyColumn).Length != 0).ToArray();
    }

    public static async Task<(WorkbookSnapshot Snapshot, string Hash, string Json)> LoadAsync(
        string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var snapshot = JsonSerializer.Deserialize<WorkbookSnapshot>(bytes, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Empty workbook snapshot.");
        if (snapshot.Version != 1 || string.IsNullOrWhiteSpace(snapshot.SpreadsheetId))
            throw new InvalidDataException("Unsupported workbook snapshot or missing provenance.");
        return (snapshot, Convert.ToHexString(SHA256.HashData(bytes)), System.Text.Encoding.UTF8.GetString(bytes));
    }
}
