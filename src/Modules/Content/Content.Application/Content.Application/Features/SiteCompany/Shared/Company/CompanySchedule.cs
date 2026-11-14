namespace Content.Application.Features.SiteCompany.Shared.Company;

/// <param name="Days">Monday = 1 … Sunday = 7.</param>
/// <param name="Opens">Time as <c>HH:mm</c>.</param>
public sealed record CompanySchedule(IReadOnlyList<int> Days, string Opens, string Closes);
