using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Crm.Application.Leads.Sheet;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace Crm.Infrastructure.Leads;

/// <summary>
/// Google Sheets API v4, read-only, with Application Default Credentials (on Cloud Run the runtime service account,
/// locally «gcloud auth application-default login»; no key file anywhere). Finds the tab by id, then reads A1:AZ in one
/// request as shown in the sheet.
/// </summary>
internal sealed class GoogleLeadSheetReader(HttpClient http, IOptions<LeadSheetOptions> options) : ILeadSheetReader
{
    private const string Api = "https://sheets.googleapis.com/v4/spreadsheets/";
    private const string ReadOnlyScope = "https://www.googleapis.com/auth/spreadsheets.readonly";
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static ITokenAccess? credential;

    public async Task<IReadOnlyList<IReadOnlyList<string?>>> ReadAsync(CancellationToken cancellationToken)
    {
        var sheet = options.Value;
        var token = await TokenAsync(cancellationToken);
        var id = Uri.EscapeDataString(sheet.SpreadsheetId);

        using var metadata = await GetAsync($"{Api}{id}?fields=sheets.properties(sheetId,title,gridProperties.rowCount)",
            token, cancellationToken);
        var (title, rowCount) = Tab(metadata.RootElement, sheet.SheetId);

        var range = Uri.EscapeDataString($"'{title.Replace("'", "''")}'!A1:AZ{rowCount}");
        using var values = await GetAsync(
            $"{Api}{id}/values/{range}?majorDimension=ROWS&valueRenderOption=FORMATTED_VALUE", token, cancellationToken);
        return Rows(values.RootElement);
    }

    private static (string Title, int Rows) Tab(JsonElement root, long sheetId)
    {
        if (root.TryGetProperty("sheets", out var sheets) && sheets.ValueKind == JsonValueKind.Array)
            foreach (var item in sheets.EnumerateArray())
            {
                var properties = item.GetProperty("properties");
                if (!properties.TryGetProperty("sheetId", out var id) || id.GetInt64() != sheetId) continue;
                var title = properties.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                var rows = properties.TryGetProperty("gridProperties", out var grid) && grid.TryGetProperty("rowCount", out var c)
                    ? c.GetInt32()
                    : 0;
                if (rows < 1 || rows > LeadSheetParser.MaxRows + 1)
                    throw new LeadSheetFormatException($"В аркуші «{title}» {rows} рядків; дозволено до {LeadSheetParser.MaxRows}.");
                return (title, rows);
            }

        throw new LeadSheetFormatException($"У таблиці «Лиды Brevi» немає аркуша з id {sheetId}.");
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

    private static async Task<string> TokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (credential is null)
            {
                await Gate.WaitAsync(cancellationToken);
                try
                {
                    credential ??= (await GoogleCredential.GetApplicationDefaultAsync(cancellationToken)).CreateScoped(ReadOnlyScope);
                }
                finally
                {
                    Gate.Release();
                }
            }

            return await credential.GetAccessTokenForRequestAsync(cancellationToken: cancellationToken);
        }
        catch (Exception error) when (error is InvalidOperationException or IOException or HttpRequestException
                                          or Google.Apis.Auth.OAuth2.Responses.TokenResponseException)
        {
            throw new LeadSheetUnavailableException("Немає облікових даних Google для читання таблиці лідів.", error);
        }
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
            throw new LeadSheetUnavailableException("Google Sheets не відповідає, спробуйте пізніше.", error);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                throw new LeadSheetUnavailableException(
                    "Google не дає доступу до таблиці «Лиды Brevi»: відкрийте її для сервісного акаунта Cloud Run (роль «Читач»).");
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new LeadSheetUnavailableException("Таблицю «Лиды Brevi» не знайдено за збереженим id.");
            if (!response.IsSuccessStatusCode)
                throw new LeadSheetUnavailableException($"Google Sheets відповів {(int)response.StatusCode}, спробуйте пізніше.");
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
    }
}
