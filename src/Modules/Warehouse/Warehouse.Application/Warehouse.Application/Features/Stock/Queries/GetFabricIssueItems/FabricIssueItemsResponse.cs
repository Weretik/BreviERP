namespace Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;

public sealed record FabricIssueItemsResponse(IReadOnlyList<FabricIssueItemResponse> Items, bool HasMore);
