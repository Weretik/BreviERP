namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

/// <summary>A fabric of «Залишки» by its canonical code with the colours the stock holds.</summary>
/// <param name="Codes">The codes of «Тканини» that are this fabric, the canonical first.</param>
/// <param name="Supplier">The supplier of the canonical offer in «Тканини», for the supplier filter.</param>
public sealed record FabricStockFabricResponse(
    string FabricCode,
    IReadOnlyList<string> Codes,
    string FabricName,
    string? Composition,
    string? Density,
    decimal Meters,
    IReadOnlyList<FabricStockColorResponse> Colors,
    string? Supplier = null,
    decimal Booked = 0,
    decimal Free = 0);
