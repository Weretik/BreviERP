namespace Catalog.Api.Contracts.Products;

public sealed record CharacteristicTableWriteRequest(string TitleUk, string TitleRu, int SortOrder, IReadOnlyCollection<CharacteristicRowWriteRequest>? Rows);
