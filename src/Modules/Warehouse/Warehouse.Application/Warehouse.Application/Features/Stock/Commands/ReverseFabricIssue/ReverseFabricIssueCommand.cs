using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricIssue;

/// <summary>«Сторно видачі»: every line goes back to the stock and the plan is reserved again.</summary>
public sealed record ReverseFabricIssueCommand(WarehouseActor Actor, int OrderId, int IssueId,
    ReverseFabricIssueInput Input) : ICommand<Result<ProductionItemFabricResponse>>;
