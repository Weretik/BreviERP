namespace Crm.Application.Abstractions.Documents;

public sealed record PublicDocumentImage(string AlternativeText, string ContentType, ReadOnlyMemory<byte> Bytes);
