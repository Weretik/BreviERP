namespace Catalog.Application.Features.Product.Shared;

public sealed record CharacteristicRowAdminDetail(
    string LabelUk,
    string LabelRu,
    string ValueUk,
    string ValueRu,
    int SortOrder);
