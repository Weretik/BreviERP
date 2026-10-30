using Crm.Application.Abstractions.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Documents.Projection;

/// <summary>Chooses the presentation pinned to a quote version; it does not read current selections.</summary>
public static class QuoteDocumentProjectionSelector
{
    public static PublicDocumentProjection Build(QuoteVersion version,
        IssuerSettingsVersion issuer, DocumentTemplateVersion template,
        string number, string publicContactName, PublicDocumentImage? logo = null)
    {
        ArgumentNullException.ThrowIfNull(version);
        return version.IntroductionPublicSnapshot is { } introduction
            ? IntroductionPublicDocumentProjectionBuilder.Build(introduction, issuer, template,
                number, version.RecipientName, publicContactName, logo)
            : QuotePublicDocumentProjectionBuilder.Build(version, issuer, template,
                number, publicContactName, logo);
    }
}
