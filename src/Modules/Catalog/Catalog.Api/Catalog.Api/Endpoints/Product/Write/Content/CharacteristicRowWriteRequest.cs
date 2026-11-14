namespace Catalog.Api.Endpoints.Product.Write.Content;

public sealed record CharacteristicRowWriteRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
