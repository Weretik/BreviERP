namespace Production.Application.Features.Reports.Shared;

/// <summary>Fabric issued to an order item in the months (issues not reversed), at the fabric's price.</summary>
public sealed record ReportIssueRowResponse(int MovementId, DateOnly IssuedOn, string FabricCode, string FabricName,
    string Color, int? OrderId, int? OrderNumber, string? ModelName, decimal Meters, decimal? PricePerMeter,
    decimal? Value);
