using BuildingBlocks.Domain.Errors;

namespace Content.Domain.SiteCompany;

/// <summary>Business rule errors of the company contacts on the site.</summary>
public static class SiteCompanyErrors
{
    public static Error InvalidPhone(string field) =>
        Error.Validation("content.company_invalid_phone", "Телефон — український номер, 10 цифр.", field);

    public static Error EmailRequired() =>
        Error.Validation("content.company_email_required", "Вкажіть пошту.", "Email");

    public static Error MapQueryRequired() =>
        Error.Validation("content.company_map_required", "Вкажіть, що шукати на мапі.", "MapQuery");

    public static Error InvalidIban() =>
        Error.Validation("content.company_invalid_iban", "Рахунок — IBAN України: UA і 27 цифр.", "BankDetails.Iban");
}
