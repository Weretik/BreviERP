namespace Catalog.Application.Contracts.Admin;

public sealed record CharacteristicRowAdminDetail(
    string LabelUk,
    string LabelRu,
    string ValueUk,
    string ValueRu,
    int SortOrder);
