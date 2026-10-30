namespace Crm.Application.Documents.Contracts;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record IssuerSettingsInput(string LegalName, string Edrpou, string TaxId, string Iban,
    string Address, string Phone, string Email, string Site, string? BankName, string? Mfo, string? LogoAssetKey);
