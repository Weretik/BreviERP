namespace Catalog.Api.Product.Write.Characteristics;

public sealed record CharacteristicRowWriteRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
