namespace Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

/// <param name="Created">New lines; a line already in the archive is counted as unchanged.</param>
public sealed record WorkshopArchiveImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<WorkshopArchiveImportError> ErrorLines);
