namespace Catalog.Application.Features.Product.Shared.Detail.Content;

public sealed record CharacteristicTableAdminDetail(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyList<CharacteristicRowAdminDetail> Rows);
