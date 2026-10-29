namespace Crm.Application.Abstractions.Documents;

// Only the explicit customer-visible projection may cross this boundary. No raw JSON/HTML/URLs.
public sealed record PublicDocumentProjection(
    string Title,
    IReadOnlyList<string> Paragraphs,
    IReadOnlyList<PublicDocumentImage> Images,
    PublicDocumentImage? HeaderLogo = null);
