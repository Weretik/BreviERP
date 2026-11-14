namespace Catalog.Application.Features.Product.Shared.Detail.Content;

public sealed record CharacteristicRowAdminDetail(
    string LabelUk,
    string LabelRu,
    string ValueUk,
    string ValueRu,
    int SortOrder);
