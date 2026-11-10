namespace Production.Domain.FabricNorms;

/// <summary>
/// The extra layers of a garment besides its top fabric, as the old «Цех» showed them in «Комплектація моделей»:
/// lining, insulation and fleece. The purpose is recognised by the material name from «Товари»; the automatic fabric
/// reservation covers only the top fabric, these have their own plan.
/// </summary>
public static class ModelLayer
{
    public const string Lining = "Підкладка";
    public const string Insulation = "Утеплювач";
    public const string Fleece = "Фліс";

    /// <returns>The layer purpose, or null when the material is not an extra layer.</returns>
    public static string? PurposeOf(string? materialName)
    {
        var name = materialName?.Trim();
        if (string.IsNullOrEmpty(name)) return null;
        if (name.StartsWith("Підкладк", StringComparison.OrdinalIgnoreCase)) return Lining;
        if (name.StartsWith("Синтепон", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Утеплювач", StringComparison.OrdinalIgnoreCase)) return Insulation;
        if (name.StartsWith("Фліс", StringComparison.OrdinalIgnoreCase)) return Fleece;
        return null;
    }
}
