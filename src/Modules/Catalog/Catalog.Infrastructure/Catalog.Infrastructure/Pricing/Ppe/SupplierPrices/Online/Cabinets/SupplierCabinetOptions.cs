namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online.Cabinets;

/// <summary>A dealer cabinet login; both values come from Secret Manager, never from files or logs.</summary>
public sealed class SupplierCabinetLogin
{
    public string? Login { get; set; }
    public string? Password { get; set; }

    public bool IsSet => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}

/// <summary>
/// <c>SupplierCabinets</c>: the Insight and Трєле dealer accounts the Apps Script kept in Script Properties
/// (INSIGHT_LOGIN, INSIGHT_PASSWORD, TRELE_LOGIN, TRELE_PASSWORD). In Cloud Run they are the secrets
/// <c>insight-login</c>, <c>insight-password</c>, <c>trele-login</c>, <c>trele-password</c>.
/// </summary>
public sealed class SupplierCabinetOptions
{
    public const string Section = "SupplierCabinets";

    public SupplierCabinetLogin Insight { get; set; } = new();
    public SupplierCabinetLogin Trele { get; set; } = new();
}
