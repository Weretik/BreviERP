namespace Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

public sealed record WorkshopArchiveImportError(string SourceKey, string Error);
