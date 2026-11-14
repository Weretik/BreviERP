namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>A planned part of an item and how far issues covered it.</summary>
/// <param name="FabricCode">The planned fabric as in the plan; the pair is its alias group.</param>
/// <param name="Layer">Upper, Lining, Insulation or Fleece.</param>
/// <param name="StockId">The first stock line of that pair; null when the stock never held it.</param>
/// <param name="StockMeters">Metres of the pair (every line of the alias group in that colour).</param>
/// <param name="StockAvailable">The pair minus what open orders hold of it; informational.</param>
/// <param name="Missing">What is still to be cut: the plan minus what issues covered.</param>
public sealed record FabricPlanPartResponse(
    string FabricCode,
    string Color,
    string ColorKey,
    string Layer,
    decimal Planned,
    decimal Covered,
    decimal Issued,
    decimal Reserved,
    int? StockId,
    decimal StockMeters,
    decimal StockAvailable,
    decimal Missing);
