namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

/// <param name="Position">The item's place in the order, from 1, as in production-orders.json.</param>
public sealed record ProductionItemProgressImportRow(int Position, IReadOnlyList<string> Stages);
