namespace Production.Application.Features.Orders.Queries.GetProductionOrders.Results;

/// <summary>
/// An item of a list row, in the order of the card: the model (code and name), its pieces and current stage, the fabric, the colours
/// (main first) and the kinds of its «нанесення» («Embroidery», «Print»).
/// </summary>
public sealed record ProductionOrderSummaryItem(string? ModelCode, string ModelName, int Quantity, string Stage, string? FabricName,
    string MainColor, IReadOnlyList<string> AdditionalColors, IReadOnlyList<string> Applications);
