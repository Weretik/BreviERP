using Production.Application.Features.Archive.Queries.GetWorkshopArchive;
namespace Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

/// <summary>A line of workshop-archive.json.</summary>
/// <param name="Kind">Schedule (the sheet «ГРАФИК работы») or Request (the old requests).</param>
/// <param name="SourceKey">Unique in the file: «schedule:12» or the old request id.</param>
public sealed record WorkshopArchiveImportRow(
    string Kind, string SourceKey, int SourceRow, DateOnly? Date, string? DateText, string? Number, string? Client,
    string? Product, string? Quantity, string? Fabric, string? Status, string? Notes,
    IReadOnlyList<WorkshopArchiveFieldResponse> Fields, IReadOnlyList<WorkshopArchiveLinkResponse> Links);
