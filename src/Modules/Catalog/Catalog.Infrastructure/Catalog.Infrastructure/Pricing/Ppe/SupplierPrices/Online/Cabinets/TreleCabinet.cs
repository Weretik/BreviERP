using System.Text.RegularExpressions;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>
/// Трєле dealer cabinet like the script's loginToTrele_: opens the sign-in page, posts login, password and
/// remember=1, follows one redirect. The dealer price is <c>data-price</c> of the add2cart__price-default element.
/// </summary>
internal sealed partial class TreleCabinet : ISupplierCabinet
{
    private static readonly Uri SignIn = new("https://trele.com.ua/login/");

    public string Host => "trele.com.ua";
    public string Name => "Трєле";

    public SupplierCabinetLogin Credentials(SupplierCabinetOptions options) => options.Trele;

    public async Task<string?> SignInAsync(SupplierCabinetClient client, SupplierCabinetLogin login,
        CancellationToken cancellationToken)
    {
        var cookies = CabinetCookies.Merge(null, (await client.GetAsync(SignIn, null, null, cancellationToken)).SetCookies);
        var response = await client.PostFormAsync(SignIn, new Dictionary<string, string>
        {
            ["login"] = login.Login!,
            ["password"] = login.Password!,
            ["remember"] = "1"
        }, cookies, null, cancellationToken);
        if (response.Status >= 400) return null;

        cookies = CabinetCookies.Merge(cookies, response.SetCookies);
        if (response.Location is not null)
            cookies = CabinetCookies.Merge(cookies,
                (await client.GetAsync(response.Location, cookies, null, cancellationToken)).SetCookies);
        return cookies.Length > 0 ? cookies : null;
    }

    public decimal? FindPrice(string html)
        => CabinetPrice().Match(html) is { Success: true } match ? SupplierPriceText.Loose(match.Groups[1].Value) : null;

    [GeneratedRegex(@"class=[""'][^""']*add2cart__price-default[^""']*[""'][^>]*data-price=[""']([^""']+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CabinetPrice();
}
