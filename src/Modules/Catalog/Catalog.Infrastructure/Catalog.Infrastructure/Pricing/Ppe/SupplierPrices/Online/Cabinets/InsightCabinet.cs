using System.Text.RegularExpressions;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>
/// Insight dealer cabinet like the script's last loginToAuthenticatedSupplier_: opens the sign-in page, tries the
/// login as JSON and as a form with both field namings, and accepts the session only when the profile page shows
/// the signed-in menu. The dealer price is <c>data-offer-mod-price0</c>, or the value inside the price0 block.
/// </summary>
internal sealed partial class InsightCabinet : ISupplierCabinet
{
    private static readonly Uri SignIn = new("https://insight.ua/authorization/");
    private static readonly Uri Profile = new("https://insight.ua/profile/");

    private static readonly IReadOnlyDictionary<string, string> Headers = new Dictionary<string, string>
    {
        ["Accept"] = "application/json, text/plain, */*",
        ["Origin"] = "https://insight.ua",
        ["Referer"] = SignIn.ToString(),
        ["X-Requested-With"] = "XMLHttpRequest"
    };

    public string Host => "insight.ua";
    public string Name => "Insight";

    public SupplierCabinetLogin Credentials(SupplierCabinetOptions options) => options.Insight;

    public async Task<string?> SignInAsync(SupplierCabinetClient client, SupplierCabinetLogin login,
        CancellationToken cancellationToken)
    {
        var cookies = CabinetCookies.Merge(null, (await client.GetAsync(SignIn, null, Headers, cancellationToken)).SetCookies);
        var attempts = new (bool Json, Dictionary<string, string> Body)[]
        {
            (true, new() { ["userEmail"] = login.Login!, ["userPassword"] = login.Password! }),
            (true, new() { ["email"] = login.Login!, ["password"] = login.Password! }),
            (false, new() { ["userEmail"] = login.Login!, ["userPassword"] = login.Password! }),
            (false, new() { ["email"] = login.Login!, ["password"] = login.Password! }),
        };

        foreach (var (json, body) in attempts)
        {
            var response = json
                ? await client.PostJsonAsync(SignIn, body, cookies, Headers, cancellationToken)
                : await client.PostFormAsync(SignIn, body, cookies, Headers, cancellationToken);
            cookies = CabinetCookies.Merge(cookies, response.SetCookies);
            if (response.Location is not null)
                cookies = CabinetCookies.Merge(cookies,
                    (await client.GetAsync(response.Location, cookies, Headers, cancellationToken)).SetCookies);

            var profile = await client.GetAsync(Profile, cookies, Headers, cancellationToken);
            cookies = CabinetCookies.Merge(cookies, profile.SetCookies);
            if (cookies.Length > 0 && SignedIn().IsMatch(profile.Body)) return cookies;
        }

        return null;
    }

    public decimal? FindPrice(string html)
    {
        if (DataPrice().Match(html) is { Success: true } data) return SupplierPriceText.Loose(data.Groups[1].Value);
        return BlockPrice().Match(html) is { Success: true } block ? SupplierPriceText.Loose(block.Groups[1].Value) : null;
    }

    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    [GeneratedRegex("Вийти|Выйти|Кабінет|Кабинет", Options)]
    private static partial Regex SignedIn();

    [GeneratedRegex(@"data-offer-mod-price0\s*=\s*[""']([^""']+)[""']", Options)]
    private static partial Regex DataPrice();

    [GeneratedRegex(@"class=[""'][^""']*price0[^""']*[""'][^>]*>[\s\S]{0,300}?class=[""'][^""']*value[^""']*[""'][^>]*>([^<]+)", Options)]
    private static partial Regex BlockPrice();
}
