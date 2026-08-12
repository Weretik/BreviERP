namespace Catalog.Application.Contracts.Admin;

public sealed record ProductPhotoDetail(
    int MediaFileId,
    string? Alt,
    bool IsVisible,
    bool IsMain,
    int SortOrder);
