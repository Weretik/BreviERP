namespace Crm.Application.Abstractions.Documents;

/// <summary>One explicit customer-visible season page of an introduction offer.</summary>
public sealed record PublicDocumentPage(
    string Heading,
    IReadOnlyList<string> IntroParagraphs,
    IReadOnlyList<PublicDocumentProductCard> Products,
    IReadOnlyList<string> FooterParagraphs);
