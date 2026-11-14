namespace Content.Application.Features.SiteCompany.Shared.Company;

/// <param name="PhotoUrl">A picture of the «Фото» library, or a picture of the site itself (<c>/assets/…</c>).</param>
public sealed record CompanyManager(CompanyText Name, string? PhotoUrl, IReadOnlyList<string> Phones, string? Email);
