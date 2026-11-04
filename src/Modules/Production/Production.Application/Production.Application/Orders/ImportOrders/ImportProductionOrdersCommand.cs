using Production.Application.Contracts;
using Production.Application.Orders.Access;

namespace Production.Application.Orders.ImportOrders;

/// <summary>
/// Moves orders of the old «Цех» (Admin only). An order already moved (same old id) stays as it is; a list with any
/// wrong order saves nothing. With <c>DryRun</c> only shows what would happen.
/// </summary>
public sealed record ImportProductionOrdersCommand(ProductionActor Actor, IReadOnlyList<ProductionOrderImportRow> Rows,
    bool DryRun) : ICommand<Result<ProductionOrderImportResult>>;
