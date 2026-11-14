namespace Catalog.Api.Endpoints.Product.Write;

public sealed record CharacteristicRowWriteRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
