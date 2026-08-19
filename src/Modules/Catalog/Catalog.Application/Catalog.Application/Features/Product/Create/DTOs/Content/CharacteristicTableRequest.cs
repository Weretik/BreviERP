namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record CharacteristicTableRequest(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyCollection<CharacteristicRowRequest> Rows);
