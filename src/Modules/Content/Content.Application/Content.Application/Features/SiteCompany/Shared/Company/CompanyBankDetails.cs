namespace Content.Application.Features.SiteCompany.Shared.Company;

public sealed record CompanyBankDetails(CompanyText Recipient, string Edrpou, string Iban, CompanyText Bank);
