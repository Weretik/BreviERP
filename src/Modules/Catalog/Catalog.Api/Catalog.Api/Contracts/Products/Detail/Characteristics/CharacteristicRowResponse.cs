namespace Catalog.Api.Contracts.Products;

public sealed record CharacteristicRowResponse(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
