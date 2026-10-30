namespace Crm.Application.CustomSewing.Production.Contracts;

public sealed record ProductionPackageFile(Guid FilePublicId, string ArchivePath,
    string ContentType, long Length, string Sha256);
