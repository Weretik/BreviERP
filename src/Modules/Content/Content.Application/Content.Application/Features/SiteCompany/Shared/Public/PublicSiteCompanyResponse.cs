using Content.Application.Features.SiteCompany.Shared.Company;

namespace Content.Application.Features.SiteCompany.Shared.Public;

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
