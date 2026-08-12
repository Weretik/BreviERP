namespace Catalog.Application.Contracts.Admin;

public sealed record CharacteristicTableAdminDetail(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyList<CharacteristicRowAdminDetail> Rows);
