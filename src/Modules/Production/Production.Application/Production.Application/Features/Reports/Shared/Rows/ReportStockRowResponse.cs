namespace Production.Application.Features.Reports.Shared.Rows;

/// <summary>The balance of a fabric and colour at the end of the last month, at the fabric's current price.</summary>
public sealed record ReportStockRowResponse(int StockId, string FabricCode, string FabricName, string Color,
    decimal Meters, decimal? PricePerMeter, decimal? Value);
