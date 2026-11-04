using Crm.Domain.Embroidery.Settings;

namespace Crm.Domain.Embroidery.Calculation;

/// <summary>
/// Cutting blanks from a roll (EMB-001, R-05): all layers are added first, rows are rounded once, no rotation.
/// A blank's cost carries its share of the whole row width, side waste included.
/// </summary>
internal static class EmbroideryCutLayout
{
    public const string BlankWiderThanRoll = "Заготовка ширша за рулон.";

    /// <returns>The layout and the exact charged amount, or null when the blank does not fit across the roll.</returns>
    public static (EmbroideryCut Cut, Rational Charged)? Plan(EmbroideryMaterial material, Rational blankWidth,
        Rational blankLength, long blanks, EmbroideryMaterialMode mode)
    {
        var rollWidth = Rational.Of(material.WidthCm);
        var price = Rational.Of(material.PricePerMetre);
        var across = (long)(rollWidth / blankWidth).Floor();
        if (across <= 0) return null;

        var rows = (long)Rational.Of(blanks, across).Ceiling();
        var metres = Rational.Of(rows) * blankLength / Rational.Of(100);
        var produced = rows * across;
        var sideWaste = rollWidth - Rational.Of(across) * blankWidth;
        var wholeCost = price * metres;
        var blankCost = price * blankLength / Rational.Of(100) / Rational.Of(across);
        var charged = mode == EmbroideryMaterialMode.FullCut ? wholeCost : Rational.Of(blanks) * blankCost;

        var cut = new EmbroideryCut(
            blankWidth.RoundHalfUp(), blankLength.RoundHalfUp(), material.WidthCm, checked((int)blanks),
            checked((int)across), checked((int)rows), checked((int)produced), checked((int)(produced - blanks)),
            sideWaste.RoundHalfUp(), metres.RoundHalfUp(), blankCost.RoundHalfUp(), wholeCost.RoundHalfUp(),
            charged.RoundHalfUp());
        return (cut, charged);
    }
}
