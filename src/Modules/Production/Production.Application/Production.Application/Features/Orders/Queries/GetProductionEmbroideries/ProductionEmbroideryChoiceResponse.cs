namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

/// <summary>A saved calculation for «Розрахунок вишивки» in the order form.</summary>
public sealed record ProductionEmbroideryChoiceResponse(
    long Id, string Number, string Name, string? CustomerName, int Quantity, int StitchesPerUnit, string HoopLabel,
    string? MockupThumbnailUrl);
