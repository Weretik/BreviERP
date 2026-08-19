namespace Catalog.Application.Contracts.Admin.Product;

public sealed record CharacteristicTableAdminDetail(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyList<CharacteristicRowAdminDetail> Rows);
