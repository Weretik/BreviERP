namespace Catalog.Application.Features.Product.Commands.CreateProduct.Content;

public sealed record CharacteristicRowRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
