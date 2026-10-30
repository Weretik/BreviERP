namespace Crm.Application.Abstractions.Documents;

/// <summary>Customer-visible paragraphs that should remain together when they fit on one page.</summary>
public sealed record PublicDocumentTextGroup(IReadOnlyList<string> Paragraphs);
