namespace Catalog.Api.Contracts.Products;

public sealed record CharacteristicRowWriteRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
