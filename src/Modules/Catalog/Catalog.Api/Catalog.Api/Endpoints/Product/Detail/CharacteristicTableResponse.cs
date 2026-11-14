namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record CharacteristicTableResponse(string TitleUk, string TitleRu, int SortOrder, IReadOnlyList<CharacteristicRowResponse> Rows);
