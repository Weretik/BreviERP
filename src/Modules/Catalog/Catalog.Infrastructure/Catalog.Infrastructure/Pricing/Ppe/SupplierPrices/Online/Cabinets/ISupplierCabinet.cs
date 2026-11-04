namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>A dealer cabinet: how to sign in and where the dealer price is on a product page.</summary>
internal interface ISupplierCabinet
{
    /// <summary>The site host; product links must point to it (with or without www.).</summary>
    string Host { get; }

    /// <summary>The supplier in the genitive case for messages: «Insight», «Трєле».</summary>
    string Name { get; }

    SupplierCabinetLogin Credentials(SupplierCabinetOptions options);

    /// <summary>The session cookie after a confirmed sign-in, or null when the site did not accept the login.</summary>
    Task<string?> SignInAsync(SupplierCabinetClient client, SupplierCabinetLogin login, CancellationToken cancellationToken);

    decimal? FindPrice(string html);
}
