namespace Catalog.Api.Product.Write.Characteristics;

public sealed record CharacteristicTableWriteRequest(string TitleUk, string TitleRu, int SortOrder, IReadOnlyCollection<CharacteristicRowWriteRequest>? Rows);
