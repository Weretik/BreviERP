namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

public sealed record FabricPlanFactModelResponse(
    string ModelName,
    string? ModelCode,
    FabricPlanFactRowResponse Summary,
    IReadOnlyList<FabricPlanFactItemResponse> Items);
