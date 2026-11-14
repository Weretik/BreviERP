namespace Content.Application.Features.SiteCompany.Shared;

public sealed record PublicCompanyManager(CompanyText Name, string? PhotoUrl, IReadOnlyList<PublicPhone> Phones, string? Email);
