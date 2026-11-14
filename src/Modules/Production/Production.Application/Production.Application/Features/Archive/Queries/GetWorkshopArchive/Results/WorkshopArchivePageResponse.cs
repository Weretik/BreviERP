namespace Production.Application.Features.Archive.Queries.GetWorkshopArchive.Results;

/// <summary>A page of «Архів цеху» (PRD-D54): read-only lines of the old «Цех».</summary>
public sealed record WorkshopArchivePageResponse(
    string Kind, int Page, int PageSize, int Total, IReadOnlyList<WorkshopArchiveRowResponse> Items);
