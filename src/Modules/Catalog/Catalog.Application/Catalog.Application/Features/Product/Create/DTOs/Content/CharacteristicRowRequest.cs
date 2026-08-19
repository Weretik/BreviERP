namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record CharacteristicRowRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
