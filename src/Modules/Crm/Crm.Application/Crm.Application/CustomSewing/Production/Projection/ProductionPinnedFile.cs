namespace Crm.Application.CustomSewing.Production.Projection;

/// <summary>Metadata pinned on the selected custom version, not current draft file flags.</summary>
public sealed record ProductionPinnedFile(Guid FilePublicId, string ContentType,
    long Length, string Sha256, bool Production);
