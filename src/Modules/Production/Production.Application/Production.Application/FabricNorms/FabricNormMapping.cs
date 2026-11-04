using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms;

internal static class FabricNormMapping
{
    public static FabricNormValues ToValues(this FabricNormInput x) => new(x.OneColor, x.TwoColorsMain,
        x.TwoColorsAdditional, x.ThreeColorsMain, x.ThreeColorsAdditional1, x.ThreeColorsAdditional2);

    public static FabricNormInput ToInput(this FabricNormValues x) => new(x.OneColor, x.TwoColorsMain,
        x.TwoColorsAdditional, x.ThreeColorsMain, x.ThreeColorsAdditional1, x.ThreeColorsAdditional2);
}
