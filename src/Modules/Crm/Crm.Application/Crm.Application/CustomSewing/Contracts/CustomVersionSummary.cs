namespace Crm.Application.CustomSewing.Contracts;

public sealed record CustomVersionSummary(Guid PublicId, int Number, Guid? ParentVersionId,
    string Name, int ActorId, DateTimeOffset CreatedAt);
