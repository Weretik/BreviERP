using System.Text;
using System.Text.Json;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>One answer of a dealer site: status, body, the cookies it set and where it redirects.</summary>
internal sealed record CabinetResponse(int Status, string Body, IReadOnlyList<string> SetCookies, Uri? Location);

/// <summary>
/// HTTP for the dealer cabinets with the script's manual cookies: no cookie container and no automatic redirects,
/// so every Set-Cookie of a login step is kept. Page reads follow up to five redirects by hand.
/// </summary>
internal sealed class SupplierCabinetClient(HttpClient client)
{
    public const string UserAgent = "Mozilla/5.0 (compatible; BreviPriceUpdater/1.1)";

    public Task<CabinetResponse> GetAsync(Uri url, string? cookies, IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken)
        => SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cookies, headers, cancellationToken);

    public Task<CabinetResponse> PostFormAsync(Uri url, IReadOnlyDictionary<string, string> form, string? cookies,
        IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
        => SendAsync(new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(form) },
            cookies, headers, cancellationToken);

    public Task<CabinetResponse> PostJsonAsync(Uri url, IReadOnlyDictionary<string, string> body, string? cookies,
        IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
        => SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        }, cookies, headers, cancellationToken);

    /// <summary>A product page with the session cookie, redirects followed; null body when it did not load.</summary>
    public async Task<SupplierPage> GetPageAsync(string url, string cookies, CancellationToken cancellationToken)
    {
        try
        {
            var target = new Uri(url);
            for (var hop = 0; hop < 5; hop++)
            {
                var response = await GetAsync(target, cookies, null, cancellationToken);
                cookies = CabinetCookies.Merge(cookies, response.SetCookies);
                if (response.Location is not null && response.Status is >= 300 and < 400)
                {
                    target = response.Location;
                    continue;
                }

                return response.Status == 200
                    ? new SupplierPage(response.Body, null)
                    : new SupplierPage(null, $"Кабінет постачальника відповів помилкою {response.Status}.");
            }

            return new SupplierPage(null, "Сторінка товару в кабінеті переадресовує по колу.");
        }
        catch (HttpRequestException)
        {
            return new SupplierPage(null, "Кабінет постачальника не відповів.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SupplierPage(null, "Кабінет постачальника не відповів за 30 секунд.");
        }
    }

    private async Task<CabinetResponse> SendAsync(HttpRequestMessage request, string? cookies,
        IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        using (request)
        {
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.TryAddWithoutValidation("Accept-Language", "uk-UA,uk;q=0.9,ru;q=0.8");
            if (!string.IsNullOrEmpty(cookies)) request.Headers.TryAddWithoutValidation("Cookie", cookies);
            foreach (var (name, value) in headers ?? new Dictionary<string, string>())
                request.Headers.TryAddWithoutValidation(name, value);

            using var response = await client.SendAsync(request, cancellationToken);
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
            var location = response.Headers.Location is { } relative
                ? relative.IsAbsoluteUri ? relative : new Uri(request.RequestUri!, relative)
                : null;
            return new CabinetResponse((int)response.StatusCode,
                await response.Content.ReadAsStringAsync(cancellationToken), setCookies, location);
        }
    }
}
