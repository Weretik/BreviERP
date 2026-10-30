namespace Crm.Application.Abstractions.Files;
public sealed record PrivateStoredFile(Guid FileId, Guid ConfigurationId, string ContentType, long Length, string Sha256);
