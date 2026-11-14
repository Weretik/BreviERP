namespace Catalog.Api.Endpoints.Product.Write.Content;

public sealed record CharacteristicTableWriteRequest(string TitleUk, string TitleRu, int SortOrder, IReadOnlyCollection<CharacteristicRowWriteRequest>? Rows);
