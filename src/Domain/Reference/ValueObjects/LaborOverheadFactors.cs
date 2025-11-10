namespace Domain.Reference.ValueObjects;

public record LaborOverheadFactors(Percent Cutting, Percent Master, Percent WorkshopManager, Percent SeamstressBonus)
{
    public static LaborOverheadFactors Default => new(
        Percent.From(21.8m),
        Percent.From(8.4m),
        Percent.From(10m),
        Percent.From(8m));
}
