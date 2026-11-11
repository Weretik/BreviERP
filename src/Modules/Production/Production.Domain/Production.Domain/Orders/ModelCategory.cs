namespace Production.Domain.Orders;

/// <summary>
/// «Категорія» of a model in «Зведення за моделями», read from its name as the old «Цех» did: a suit with a
/// semi-overall or with trousers, a jacket, a semi-overall, an overall, trousers, a vest, a fleece, else «Інше».
/// </summary>
public static class ModelCategory
{
    public const string Other = "Інше";

    private static readonly (string Word, string Category)[] Kinds =
    [
        ("напівкомбінезон", "Напівкомбінезон"),
        ("комбінезон", "Комбінезон"),
        ("куртка", "Куртка"),
        ("штани", "Штани"),
        ("брюки", "Штани"),
        ("жилет", "Жилет"),
        ("фліс", "Фліска"),
    ];

    public static string Of(string? modelName)
    {
        var name = (modelName ?? string.Empty).Trim().ToLowerInvariant();
        if (name.Length == 0) return Other;
        if (name.Contains("костюм") || name.Contains('+') || name.Contains(" / "))
            return name.Contains("напівкомбінезон") ? "Костюм куртка + напівкомбінезон" : "Костюм куртка + брюки";
        foreach (var (word, category) in Kinds)
            if (name.StartsWith(word, StringComparison.Ordinal)) return category;
        foreach (var (word, category) in Kinds)
            if (name.Contains(word, StringComparison.Ordinal)) return category;
        return Other;
    }
}
