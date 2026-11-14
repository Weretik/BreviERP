namespace Catalog.Application.Features.Product.Commands.CreateProduct.Content;

public sealed record CharacteristicTableRequest(
    string TitleUk,
    string TitleRu,
    int SortOrder,
    IReadOnlyCollection<CharacteristicRowRequest> Rows);
