using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crm.Application.Customers.KeepinCrmSync;
using Microsoft.Extensions.Options;

namespace Crm.Infrastructure.KeepinCrm;

/// <summary>GET {BaseUrl}clients?page=N&amp;q[lead_false]=true with header X-Auth-Token, until pagination.total_pages.</summary>
internal sealed class KeepinCrmClientHttpSource(HttpClient http, IOptions<KeepinCrmClientOptions> options)
    : IKeepinCrmClientSource
{
    public async Task<IReadOnlyList<KeepinCrmClientSnapshot>> GetAllAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new KeepinCrmClientsUnavailableException("KeepinCRM не налаштовано: немає ключа API (KeepinCrm__ApiKey).");

        var baseUrl = settings.BaseUrl.EndsWith('/') ? settings.BaseUrl : settings.BaseUrl + "/";
        var all = new List<KeepinCrmClientSnapshot>();
        for (var page = 1; page <= settings.ClientMaxPages; page++)
        {
            var url = new Uri($"{baseUrl}clients?page={page}&q%5Blead_false%5D=true");
            var parsed = KeepinCrmClientParser.ParsePage(await GetPageAsync(url, settings.ApiKey, cancellationToken));
            all.AddRange(parsed.Clients);
            if (parsed.Rows == 0 || page >= (parsed.TotalPages ?? page))
                return all;
        }

        throw new KeepinCrmClientsUnavailableException(
            $"KeepinCRM має більше ніж {settings.ClientMaxPages} сторінок клієнтів. Дані в Brevi не змінено.");
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
            throw new KeepinCrmClientsUnavailableException("KeepinCRM недоступний. Дані в Brevi не змінено.", error);
        }
        catch (TaskCanceledException error) when (!cancellationToken.IsCancellationRequested)
        {
            throw new KeepinCrmClientsUnavailableException("KeepinCRM не відповів вчасно. Дані в Brevi не змінено.", error);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new KeepinCrmClientsUnavailableException(
                    $"KeepinCRM повернув HTTP {(int)response.StatusCode}. Дані в Brevi не змінено.");

            try
            {
                return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            }
            catch (JsonException error)
            {
                throw new KeepinCrmClientsUnavailableException("KeepinCRM повернув некоректну відповідь JSON.", error);
            }
        }
    }
}
