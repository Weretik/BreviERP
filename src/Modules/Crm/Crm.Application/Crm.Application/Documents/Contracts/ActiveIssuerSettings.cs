namespace Crm.Application.Documents.Contracts;

public sealed record ActiveIssuerSettings(long Revision, IssuerSettingsDetail? Active);
