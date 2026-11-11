namespace Content.Application.Contracts;

/// <summary>A text in Ukrainian and Russian.</summary>
public sealed record CompanyText(string Uk, string Ru);

/// <param name="Days">Monday = 1 … Sunday = 7.</param>
/// <param name="Opens">Time as <c>HH:mm</c>.</param>
public sealed record CompanySchedule(IReadOnlyList<int> Days, string Opens, string Closes);

/// <param name="PhotoUrl">A picture of the «Фото» library, or a picture of the site itself (<c>/assets/…</c>).</param>
public sealed record CompanyManager(CompanyText Name, string? PhotoUrl, IReadOnlyList<string> Phones, string? Email);

public sealed record CompanyBankDetails(CompanyText Recipient, string Edrpou, string Iban, CompanyText Bank);

public sealed record CompanySocials(string? Instagram, string? Youtube, string? Facebook);

/// <summary>«Контент → Контакти та компанія» (CNT-009). <paramref name="Revision"/> is 0 before the first save.</summary>
public sealed record SiteCompanyResponse(
    string Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<CompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials,
    int Revision,
    DateTimeOffset? UpdatedAtUtc,
    string? UpdatedBy);

/// <param name="E164">The <c>tel:</c> value, <c>+380504698675</c>.</param>
/// <param name="Display">As the site shows it, <c>050 469 86 75</c>.</param>
public sealed record PublicPhone(string E164, string Display);

public sealed record PublicCompanyManager(CompanyText Name, string? PhotoUrl, IReadOnlyList<PublicPhone> Phones, string? Email);

/// <summary>What the public site shows about the company: header, footer, «Контакти», payment details, structured data.</summary>
public sealed record PublicSiteCompanyResponse(
    PublicPhone Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<PublicCompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials);
