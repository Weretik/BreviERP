namespace Production.Application.Features.Archive.Queries.GetWorkshopArchive;

/// <param name="Date">Null when the old book had no readable date; <c>DateText</c> keeps what was written.</param>
/// <param name="Fields">Other columns of the old book, such as «Сезон» or «Пошив».</param>
public sealed record WorkshopArchiveRowResponse(
    int Id, int SourceRow, DateOnly? Date, string? DateText, string? Number, string? Client, string? Product,
    string? Quantity, string? Fabric, string? Status, string? Notes, IReadOnlyList<WorkshopArchiveFieldResponse> Fields,
    IReadOnlyList<WorkshopArchiveLinkResponse> Links);
