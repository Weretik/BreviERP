namespace Catalog.Api.Contracts.Products;

public sealed record CharacteristicTableResponse(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowResponse> Rows);
