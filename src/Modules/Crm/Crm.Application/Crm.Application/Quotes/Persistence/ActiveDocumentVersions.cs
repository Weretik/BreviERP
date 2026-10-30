using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;

namespace Crm.Application.Quotes.Persistence;

public sealed record ActiveDocumentVersions(IssuerSettingsVersion Issuer, DocumentTemplateVersion Template);
