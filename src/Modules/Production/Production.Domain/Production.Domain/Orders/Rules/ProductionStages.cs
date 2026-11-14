using System.Globalization;
using Production.Domain.Orders.Enums;

namespace Production.Domain.Orders.Rules;

/// <summary>Ukrainian labels of the stages, for history lines and the Telegram text.</summary>
public static class ProductionStages
{
    public static string Label(ProductionStage stage) => stage switch
    {
        ProductionStage.New => "Нове",
        ProductionStage.FabricOrdered => "Тканина замовлена",
        ProductionStage.FabricAvailable => "Тканина в наявності",
        ProductionStage.Cutting => "Крій",
        ProductionStage.Application => "Нанесення",
        ProductionStage.Sewing => "Пошив",
        ProductionStage.Ready => "Готово до відвантаження",
        ProductionStage.Shipped => "Фактично відвантажено",
        ProductionStage.Complete => "Завершено",
        _ => ((int)stage).ToString(CultureInfo.InvariantCulture)
    };

    /// <summary>Distinct stages in workshop order; nothing ticked means «Нове».</summary>
    public static List<ProductionStage> Normalize(IEnumerable<ProductionStage> stages)
    {
        var list = stages.Where(Enum.IsDefined).Distinct().Order().ToList();
        return list.Count == 0 ? [ProductionStage.New] : list;
    }

    public static string Describe(IEnumerable<ProductionStage> stages) => string.Join(", ", stages.Select(Label));
}
