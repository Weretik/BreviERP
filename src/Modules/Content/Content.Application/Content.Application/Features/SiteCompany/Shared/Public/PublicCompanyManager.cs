using Content.Application.Features.SiteCompany.Shared.Company;

namespace Content.Application.Features.SiteCompany.Shared.Public;

public sealed record PublicCompanyManager(CompanyText Name, string? PhotoUrl, IReadOnlyList<PublicPhone> Phones, string? Email);
