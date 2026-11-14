using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteCompany.Shared.Company;

namespace Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;

/// <summary>Replaces the company facts as a whole with a revision check (CNT-009 SC-02).</summary>
public sealed record SaveSiteCompanyCommand(
    int Revision,
    string Phone,
    string Email,
    CompanyText City,
    CompanyText Street,
    string MapQuery,
    CompanySchedule Schedule,
    IReadOnlyList<CompanyManager> Managers,
    CompanyBankDetails BankDetails,
    CompanySocials Socials,
    string UpdatedBy) : ICommand<Result<SiteCompanyResponse>>;
