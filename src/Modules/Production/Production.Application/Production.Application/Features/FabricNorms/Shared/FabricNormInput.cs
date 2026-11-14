namespace Production.Application.Features.FabricNorms.Shared;

/// <summary>Planned fabric per item in metres; see Production.Domain.FabricNorms.FabricNormValues.</summary>
public sealed record FabricNormInput(
    decimal? OneColor,
    decimal? TwoColorsMain,
    decimal? TwoColorsAdditional,
    decimal? ThreeColorsMain,
    decimal? ThreeColorsAdditional1,
    decimal? ThreeColorsAdditional2);
