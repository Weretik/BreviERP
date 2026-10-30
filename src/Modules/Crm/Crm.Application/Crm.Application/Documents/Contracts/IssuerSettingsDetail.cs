namespace Crm.Application.Documents.Contracts;

public sealed record IssuerSettingsDetail(long Revision, bool IsActive, Guid IssuerVersionId,
    Guid TemplateVersionId, int VersionNumber, IssuerSettingsInput Issuer,
    DocumentTemplateInput Template, DateTimeOffset CreatedAt);
