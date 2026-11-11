namespace Production.Domain.FabricPlans;

/// <summary>A fabric colour as staff type it; the key ignores case and extra spaces so «Темно-синій» and «темно-синій » meet.</summary>
public static class PlanColor
{
    public const int MaxLength = 80;

    public static string Clean(string? color) => ProductionText.Clean(color, MaxLength);

    public static string Key(string color) => ProductionText.Key(color);
}
