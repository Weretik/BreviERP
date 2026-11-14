namespace Production.Application.Features.Reports.Shared;

/// <summary>
/// Money counts only what has a price: pieces without the КП price and metres without a fabric price are given apart.
/// </summary>
public sealed record WorkshopReportTotalsResponse(
    int SewnPieces,
    decimal OutputValue,
    int UnpricedPieces,
    decimal Accrued,
    int Minutes,
    decimal IssuedMeters,
    decimal IssuedValue,
    decimal UnpricedIssuedMeters,
    decimal StockMeters,
    decimal StockValue,
    decimal UnpricedStockMeters);
