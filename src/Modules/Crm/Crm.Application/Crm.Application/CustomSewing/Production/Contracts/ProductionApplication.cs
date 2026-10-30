namespace Crm.Application.CustomSewing.Production.Contracts;

public sealed record ProductionApplication(string Kind, string Location, Guid? ArtworkFileId);
