using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion;
using Purchasing.Domain.Ingestion.Errors;

namespace Purchasing.Infrastructure.Integrations.GoogleSheets;

/// <summary>
/// Google Sheets API v4, read-only: checks the sheet id, title, grid size and exact A:N header, then reads all rows
/// in one request. Values are read as shown in the sheet (formatted), so ids keep their leading zeros.
/// </summary>
internal sealed class GooglePurchaseSheetReader(
    HttpClient http,
    IGoogleAccessTokenSource tokens,
    IOptions<PurchaseSheetOptions> options) : IPurchaseSheetReader
{
    private const string Api = "https://sheets.googleapis.com/v4/spreadsheets/";

    public async Task<PurchaseSheetContent> ReadAsync(CancellationToken cancellationToken)
    {
        var sheet = options.Value;
        var token = await tokens.GetTokenAsync(cancellationToken);

        using var metadata = await GetAsync(
            $"{Api}{Uri.EscapeDataString(sheet.SpreadsheetId)}?fields=sheets.properties(sheetId,title,gridProperties.rowCount)",
            token, cancellationToken);
        var rowCount = GridRowCount(metadata.RootElement, sheet);

        var range = Uri.EscapeDataString($"'{sheet.SheetTitle}'!A1:N{rowCount}");
        using var values = await GetAsync(
            $"{Api}{Uri.EscapeDataString(sheet.SpreadsheetId)}/values/{range}?majorDimension=ROWS&valueRenderOption=FORMATTED_VALUE",
            token, cancellationToken);
        var rows = Rows(values.RootElement);
        CheckHeader(rows.Count > 0 ? rows[0] : []);
        return new PurchaseSheetContent(sheet.SpreadsheetId, sheet.SheetId, rows.Skip(1).ToList());
    }

    private static int GridRowCount(JsonElement root, PurchaseSheetOptions sheet)
    {
        if (root.TryGetProperty("sheets", out var sheets) && sheets.ValueKind == JsonValueKind.Array)
            foreach (var item in sheets.EnumerateArray())
            {
                var properties = item.GetProperty("properties");
                if (!properties.TryGetProperty("sheetId", out var id) || id.GetInt64() != sheet.SheetId) continue;
                var title = properties.TryGetProperty("title", out var t) ? t.GetString() : null;
                if (title != sheet.SheetTitle)
                    throw new PurchaseSourceException($"Аркуш {sheet.SheetId} називається «{title}», а очікується «{sheet.SheetTitle}».");
                var rows = properties.TryGetProperty("gridProperties", out var grid) && grid.TryGetProperty("rowCount", out var count)
                    ? count.GetInt32()
                    : 0;
                if (rows < 1 || rows > PurchaseSheetColumns.MaxGridRows)
                    throw new PurchaseSourceException($"В аркуші «{title}» {rows} рядків; дозволено від 1 до {PurchaseSheetColumns.MaxGridRows}.");
                return rows;
            }

        throw new PurchaseSourceException($"У таблиці немає аркуша з id {sheet.SheetId} («{sheet.SheetTitle}»).");
    }

    private static List<IReadOnlyList<string?>> Rows(JsonElement root)
    {
        var rows = new List<IReadOnlyList<string?>>();
        if (!root.TryGetProperty("values", out var values) || values.ValueKind != JsonValueKind.Array) return rows;
        foreach (var row in values.EnumerateArray())
            rows.Add(row.EnumerateArray().Select(cell => cell.ValueKind switch
            {
                JsonValueKind.String => cell.GetString(),
                JsonValueKind.Null => null,
                _ => cell.GetRawText(),
            }).ToList());
        return rows;
    }

    private static void CheckHeader(IReadOnlyList<string?> header)
    {
        var expected = PurchaseSheetColumns.Header;
        if (header.Count == expected.Count && header.Select((x, i) => x == expected[i]).All(x => x)) return;
        throw new PurchaseSourceException(
            $"Заголовок аркуша A1:N1 не збігається. Очікується: {string.Join(", ", expected)}; у таблиці: {string.Join(", ", header)}.");
    }

    private async Task<JsonDocument> GetAsync(string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new PurchaseSheetUnavailableException("Google Sheets не відповідає, спробуйте пізніше.", error);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                throw new PurchaseSheetUnavailableException(
                    "Google не дає доступу до таблиці «БРЕВІ команда»: відкрийте її для сервісного акаунта Cloud Run (роль «Читач») і перевірте, що Google Sheets API увімкнено.");
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new PurchaseSheetUnavailableException("Таблицю «БРЕВІ команда» не знайдено за збереженим id.");
            if (!response.IsSuccessStatusCode)
                throw new PurchaseSheetUnavailableException($"Google Sheets відповів {(int)response.StatusCode}, спробуйте пізніше.");
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
    }
}
