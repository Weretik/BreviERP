namespace Catalog.Api.Endpoints.Product.Write;

public sealed record CharacteristicTableWriteRequest(string TitleUk, string TitleRu, int SortOrder, IReadOnlyCollection<CharacteristicRowWriteRequest>? Rows);
