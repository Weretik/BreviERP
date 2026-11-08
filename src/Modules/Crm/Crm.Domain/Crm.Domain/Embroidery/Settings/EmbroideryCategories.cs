namespace Crm.Domain.Embroidery.Settings;

/// <summary>
/// The two price categories of «Ціна вишивки» (round 2 of the client corrections, 2026-11-08): «Опт» was removed,
/// «Стандарт» became «Категорія 1» and «Терміново» «Категорія 2». Calculations saved before keep their prices under
/// the old names; <see cref="Label"/> shows those names in the new way.
/// </summary>
public static class EmbroideryCategories
{
    public const string First = "Категорія 1";
    public const string Second = "Категорія 2";

    /// <summary>The price a calculation saved before the change stands for: «Стандарт», shown as «Категорія 1».</summary>
    public const string LegacyChosen = "Стандарт";

    /// <summary>A price name as the pages show it: the old «Стандарт» and «Терміново» get their category names.</summary>
    public static string Label(string name) => name switch
    {
        LegacyChosen => First,
        "Терміново" => Second,
        _ => name,
    };
}
