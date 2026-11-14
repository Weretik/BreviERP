namespace Production.Application.Features.Orders.Queries.GetProductionModelSummary;

/// <summary>A model of «Зведення за моделями»: ordered pieces over the filtered orders.</summary>
/// <param name="ModelCode">Null for an item without a Catalog model (old orders); then the name groups it.</param>
/// <param name="Season">Catalog season of the model; null when unknown.</param>
/// <param name="Category">Read from the model name: «Куртка», «Костюм куртка + брюки»…, else «Інше».</param>
public sealed record ProductionModelSummaryRow(
    string? ModelCode, string ModelName, string? Season, string Category, int Quantity, int OrdersCount);
