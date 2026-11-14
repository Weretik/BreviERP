namespace Catalog.Application.Features.Product.Shared;

public sealed record CharacteristicTableAdminDetail(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyList<CharacteristicRowAdminDetail> Rows);
