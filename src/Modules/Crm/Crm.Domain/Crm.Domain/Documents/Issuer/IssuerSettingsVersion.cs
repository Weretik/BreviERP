using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Documents.Issuer;

public sealed class IssuerSettingsVersion : BaseEntity<IssuerSettingsVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public int Number { get; private set; }
    public string LegalName { get; private set; } = null!;
    public string Edrpou { get; private set; } = null!;
    public string TaxId { get; private set; } = null!;
    public string Iban { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Site { get; private set; } = null!;
    public string? BankName { get; private set; }
    public string? Mfo { get; private set; }
    public string? LogoAssetKey { get; private set; }
    public CrmUserId VerifiedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private IssuerSettingsVersion() { }

    public static IssuerSettingsVersion Create(int number, string legalName, string edrpou, string taxId,
        string iban, string address, string phone, string email, string site, string? bankName,
        string? mfo, string? logoAssetKey, CrmUserId verifiedBy)
    {
        if (number <= 0 || verifiedBy.Value <= 0) throw new DocumentSettingsValidationException("invalid_issuer_version");
        var nameValue = Required(legalName, 200, "legal_name_required");
        var edrpouValue = Required(edrpou, 50, "edrpou_required");
        var taxValue = Required(taxId, 50, "tax_id_required");
        var ibanValue = Required(iban, 80, "iban_required");
        var addressValue = Required(address, 500, "address_required");
        var phoneValue = Required(phone, 80, "phone_required");
        var emailValue = Required(email, 200, "email_required");
        var siteValue = Required(site, 500, "site_required");
        if (!System.Net.Mail.MailAddress.TryCreate(emailValue, out var mail) || mail.Address != emailValue)
            throw new DocumentSettingsValidationException("invalid_email");
        if (!Uri.TryCreate(siteValue, UriKind.Absolute, out var siteUri) || siteUri.Scheme != Uri.UriSchemeHttps)
            throw new DocumentSettingsValidationException("invalid_site");
        var logo = Optional(logoAssetKey, 200);
        // Logical keys are resolved only by the server's provisioned asset map.
        if (logo is not null && logo != "brevi-wordmark")
            throw new DocumentSettingsValidationException("invalid_logo_asset_key");
        return new IssuerSettingsVersion
        {
            Number = number, LegalName = nameValue, Edrpou = edrpouValue, TaxId = taxValue,
            Iban = ibanValue, Address = addressValue, Phone = phoneValue, Email = emailValue,
            Site = siteValue, BankName = Optional(bankName, 200), Mfo = Optional(mfo, 20),
            LogoAssetKey = logo, VerifiedBy = verifiedBy
        };
    }

    private static string Required(string? value, int maxLength, string code)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > maxLength)
            throw new DocumentSettingsValidationException(code);
        return trimmed;
    }

    private static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        if (trimmed.Length > maxLength) throw new DocumentSettingsValidationException("field_too_long");
        return trimmed;
    }
}
