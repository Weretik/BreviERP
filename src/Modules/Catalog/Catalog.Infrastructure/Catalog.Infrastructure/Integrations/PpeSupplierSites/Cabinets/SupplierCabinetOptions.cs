namespace Catalog.Infrastructure.Integrations.PpeSupplierSites.Cabinets;

/// <summary>A dealer cabinet login; both values come from Secret Manager, never from files or logs.</summary>
public sealed class SupplierCabinetLogin
{
    public string? Login { get; set; }
    public string? Password { get; set; }

    public bool IsSet => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}

/// <summary>
/// <c>SupplierCabinets</c>: the Трєле dealer account the Apps Script kept in Script Properties (TRELE_LOGIN,
/// TRELE_PASSWORD). In Cloud Run they are the secrets <c>trele-login</c> and <c>trele-password</c>. Insight is
/// updated only from its XML files (owner's decision, 2026-11-04).
/// </summary>
public sealed class SupplierCabinetOptions
{
    public const string Section = "SupplierCabinets";

    public SupplierCabinetLogin Trele { get; set; } = new();
}
