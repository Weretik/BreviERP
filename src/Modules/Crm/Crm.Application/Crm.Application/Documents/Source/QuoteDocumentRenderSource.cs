using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Documents.Source;

public sealed record QuoteDocumentRenderSource(QuoteIssue Issue, QuoteVersion Version,
    IssuerSettingsVersion Issuer, DocumentTemplateVersion Template, string PublicContactName);
