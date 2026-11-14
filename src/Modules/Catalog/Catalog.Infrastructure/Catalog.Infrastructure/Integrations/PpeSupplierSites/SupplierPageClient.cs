using System.Net;

namespace Catalog.Infrastructure.Integrations.PpeSupplierSites;

/// <summary>A page or price list downloaded from a supplier; <see cref="Text"/> is null when it did not load.</summary>
internal sealed record SupplierPage(string? Text, string? Problem);

/// <summary>Downloads supplier pages and published price lists, following redirects, with the script's user agent.</summary>
internal sealed class SupplierPageClient(HttpClient client)
{
    public async Task<SupplierPage> GetAsync(string url, string userAgent, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
                return new SupplierPage(null, $"Сайт постачальника відповів помилкою {(int)response.StatusCode}.");
            return new SupplierPage(await response.Content.ReadAsStringAsync(cancellationToken), null);
        }
        catch (HttpRequestException)
        {
            return new SupplierPage(null, "Сайт постачальника не відповів.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SupplierPage(null, "Сайт постачальника не відповів за 30 секунд.");
        }
    }
}
