namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>The script's cookie handling: name=value pairs from Set-Cookie, a later value replaces an earlier one.</summary>
internal static class CabinetCookies
{
    public static string Merge(string? first, IEnumerable<string> setCookies)
    {
        var byName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in (first ?? string.Empty).Split(';').Concat(setCookies.Select(x => x.Split(';')[0])))
        {
            var trimmed = part.Trim();
            var separator = trimmed.IndexOf('=');
            if (separator > 0) byName[trimmed[..separator]] = trimmed;
        }

        return string.Join("; ", byName.Values);
    }
}
