using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteCompany.Shared.Company;

namespace Content.Api.Endpoints.SiteCompany.SaveSiteCompany;

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
