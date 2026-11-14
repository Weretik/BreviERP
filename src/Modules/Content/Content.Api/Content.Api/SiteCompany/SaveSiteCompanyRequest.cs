using Content.Application.Features.SiteCompany.Shared;

namespace Content.Api.SiteCompany;

public sealed record SaveSiteCompanyRequest(
    int Revision,
    string Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<CompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials);
