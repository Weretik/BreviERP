namespace Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

/// <param name="Purpose">Підкладка, Утеплювач or Фліс.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ModelLayerResponse(string Purpose, string? Code, string? Name, decimal Quantity, string Unit);
