using Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;

namespace Production.Application.Features.Orders.Commands.ImportProductionOrders;

public sealed record ProductionOrderImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<ProductionOrderImportLine> Lines);
