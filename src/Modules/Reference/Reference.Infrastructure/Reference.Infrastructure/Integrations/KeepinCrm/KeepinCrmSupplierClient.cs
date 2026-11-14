using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Reference.Domain.Suppliers;
using Reference.Application.Abstractions;
using Reference.Application.Abstractions.KeepinCrm;

namespace Reference.Infrastructure.Integrations.KeepinCrm;

/// <summary>GET {BaseUrl}suppliers?page=N with header X-Auth-Token, page by page, as the sheet script did.</summary>
internal sealed class KeepinCrmSupplierClient(HttpClient http, IOptions<KeepinCrmOptions> options)
    : IKeepinCrmSupplierSource
{
    public async Task<IReadOnlyList<KeepinCrmSupplierSnapshot>> GetAllAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new KeepinCrmUnavailableException("KeepinCRM не налаштовано: немає ключа API (KeepinCrm__ApiKey).");

        var baseUrl = settings.BaseUrl.EndsWith('/') ? settings.BaseUrl : settings.BaseUrl + "/";
        var all = new List<KeepinCrmSupplierSnapshot>();
        for (var page = 1; page <= settings.MaxPages; page++)
        {
            var payload = await GetPageAsync(new Uri($"{baseUrl}suppliers?page={page}"), settings.ApiKey, cancellationToken);
            var rows = KeepinCrmSupplierParser.CountRows(payload);
            if (rows == 0)
                break;

            all.AddRange(KeepinCrmSupplierParser.ParsePage(payload));
            if (rows < settings.PageSize)
                break;
        }

        return all;
    }

    private async Task<JsonNode?> GetPageAsync(Uri url, string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Auth-Token", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException error)
        {
            throw new KeepinCrmUnavailableException("KeepinCRM недоступний. Дані в Brevi не змінено.", error);
        }
        catch (TaskCanceledException error) when (!cancellationToken.IsCancellationRequested)
        {
            throw new KeepinCrmUnavailableException("KeepinCRM не відповів вчасно. Дані в Brevi не змінено.", error);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new KeepinCrmUnavailableException(
                    $"KeepinCRM повернув HTTP {(int)response.StatusCode}. Дані в Brevi не змінено.");
            }

            try
            {
                return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            }
            catch (JsonException error)
            {
                throw new KeepinCrmUnavailableException("KeepinCRM повернув некоректну відповідь JSON.", error);
            }
        }
    }
}
