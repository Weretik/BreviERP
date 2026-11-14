using Content.Application.Features.SiteCompany.Shared.Company;

namespace Content.Application.Features.SiteCompany.Shared;

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
