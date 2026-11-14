using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online;

/// <summary>Product links to read: each link once, even when sizes of a product share it.</summary>
internal static class SupplierLinks
{
    public static IReadOnlyList<string> Distinct(IReadOnlyList<PpePriceImportProduct> products)
        => products
            .Select(product => product.SupplierUrl?.Trim())
            .Where(url => !string.IsNullOrEmpty(url))
            .Select(url => url!)
            .DistinctBy(url => SupplierKeys.Url(url))
            .ToList();

    /// <summary>A web link on the supplier's site or its www. subdomain, like the script's host checks.</summary>
    public static bool IsOnSite(string url, string host)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && uri.Scheme is "https" or "http"
           && (uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase)
               || uri.Host.Equals("www." + host, StringComparison.OrdinalIgnoreCase));
}
