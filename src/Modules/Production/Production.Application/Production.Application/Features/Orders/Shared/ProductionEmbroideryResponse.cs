using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
namespace Production.Application.Features.Orders.Shared;

/// <summary>The saved «Ціна вишивки» calculation of an embroidery, for the workshop; no prices.</summary>
/// <param name="Mode">`Garment` or `Chevron`.</param>
/// <param name="StabilizerMetres">Флізелін to cut; shown only, the stock is not changed.</param>
/// <param name="FabricMetres">Fabric under the chevrons; null for embroidery on a garment.</param>
public sealed record ProductionEmbroideryResponse(
    long Id, string Number, string Name, string? CustomerName, string Mode, int Quantity, int StitchesPerUnit, string HoopLabel,
    int Hoopings, decimal? StabilizerMetres, decimal? FabricMetres, string? MockupUrl, string? MockupThumbnailUrl)
{
    public ProductionEmbroideryChoiceResponse ToChoice() =>
        new(Id, Number, Name, CustomerName, Quantity, StitchesPerUnit, HoopLabel, MockupThumbnailUrl);
}
