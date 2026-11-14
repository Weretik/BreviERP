namespace Catalog.Api.Product.Detail.Characteristics;

public sealed record CharacteristicTableResponse(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowResponse> Rows);
