namespace Content.Application.Features.SiteCompany.Shared;

public sealed record CompanyBankDetails(CompanyText Recipient, string Edrpou, string Iban, CompanyText Bank);
