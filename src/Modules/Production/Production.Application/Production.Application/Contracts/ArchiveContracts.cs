namespace Production.Application.Contracts;

/// <summary>A page of «Архів цеху» (PRD-D54): read-only lines of the old «Цех».</summary>
public sealed record WorkshopArchivePageResponse(
    string Kind, int Page, int PageSize, int Total, IReadOnlyList<WorkshopArchiveRowResponse> Items);

/// <param name="Date">Null when the old book had no readable date; <c>DateText</c> keeps what was written.</param>
/// <param name="Fields">Other columns of the old book, such as «Сезон» or «Пошив».</param>
public sealed record WorkshopArchiveRowResponse(
    int Id, int SourceRow, DateOnly? Date, string? DateText, string? Number, string? Client, string? Product,
    string? Quantity, string? Fabric, string? Status, string? Notes, IReadOnlyList<WorkshopArchiveFieldResponse> Fields,
    IReadOnlyList<WorkshopArchiveLinkResponse> Links);

public sealed record WorkshopArchiveFieldResponse(string Label, string Value);

public sealed record WorkshopArchiveLinkResponse(string Label, string Url);

/// <summary>A line of workshop-archive.json.</summary>
/// <param name="Kind">Schedule (the sheet «ГРАФИК работы») or Request (the old requests).</param>
/// <param name="SourceKey">Unique in the file: «schedule:12» or the old request id.</param>
public sealed record WorkshopArchiveImportRow(
    string Kind, string SourceKey, int SourceRow, DateOnly? Date, string? DateText, string? Number, string? Client,
    string? Product, string? Quantity, string? Fabric, string? Status, string? Notes,
    IReadOnlyList<WorkshopArchiveFieldResponse> Fields, IReadOnlyList<WorkshopArchiveLinkResponse> Links);

/// <param name="Created">New lines; a line already in the archive is counted as unchanged.</param>
public sealed record WorkshopArchiveImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<WorkshopArchiveImportError> ErrorLines);

public sealed record WorkshopArchiveImportError(string SourceKey, string Error);
