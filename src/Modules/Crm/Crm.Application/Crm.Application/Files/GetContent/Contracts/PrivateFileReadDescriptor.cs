namespace Crm.Application.Files.GetContent.Contracts;

public sealed record PrivateFileReadDescriptor(Guid FileId, Guid DraftId, string ContentType, long Length, string Sha256);
