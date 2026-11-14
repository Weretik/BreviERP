using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact.Responses;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

/// <summary>An evaluated item and the model it is grouped by.</summary>
/// <param name="InProgress">Issues did not cover all the products yet: left out of the summary.</param>
public sealed record FabricPlanFactEntry(string ModelName, string? ModelCode, bool InProgress,
    FabricPlanFactItemResponse Item);
