namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

/// <summary>The supplier price sources that the client's Apps Script handled, with their logic and default discounts.</summary>
public static class PpePriceSources
{
    public const string AvCenter = "av-center";
    public const string ThreeCMarket = "3c-market";
    public const string PerchatkaCentr = "perchatka-centr";
    public const string Insight = "insight";
    public const string Trele = "trele";
    public const string Cemto = "cemto";

    public static readonly IReadOnlyList<PpePriceSource> All =
    [
        new(AvCenter, "АВ-Центр", ["АВ-Центр", "AV-Center", "АВ Центр"], PpePriceSourceMethod.Website,
            "Щопонеділка о 07:00",
            "Відкриває сторінку кожного товару за посиланням постачальника, бере ціну з сайту і віднімає знижку.",
            20m, PpePriceFileKind.None, 0, PpePriceUpdateCadence.WeeklyMonday, true),
        new(ThreeCMarket, "3C-Маркет", ["3C-Маркет", "3C-Макркет", "3С-Маркет", "3C Market"], PpePriceSourceMethod.GoogleSheets,
            "Щопонеділка о 07:00",
            "Читає 6 опублікованих Google-таблиць 3C, знаходить товар за артикулом постачальника і віднімає знижку. Якщо одна таблиця не відкрилася, ціни не змінюються.",
            5m, PpePriceFileKind.None, 0, PpePriceUpdateCadence.WeeklyMonday, true),
        new(PerchatkaCentr, "Перчатка-центр", ["Перчатка-центр", "Перчатка центр"], PpePriceSourceMethod.Website,
            "Щопонеділка о 07:00",
            "Відкриває сторінку кожного товару за посиланням постачальника, бере ціну з сайту і віднімає знижку.",
            22m, PpePriceFileKind.None, 0, PpePriceUpdateCadence.WeeklyMonday, true),
        new(Insight, "Insight", ["Інсайт", "Insight", "Инсайт"], PpePriceSourceMethod.SupplierCabinet,
            "Щодня о 08:00 через кабінет дилера; вручну — трьома XML",
            "Три XML-прайси Insight: найменша ціна товару — дилерська (ціна постачальника), середня — опт, найбільша — роздріб. Товар знаходиться за посиланням постачальника або артикулом. Опт і роздріб мають бути щонайменше на 20% вищі за дилерську, роздріб — не нижчий за опт.",
            0m, PpePriceFileKind.InsightYml, 3, PpePriceUpdateCadence.Daily, false),
        new(Trele, "Трєле", ["Трєле", "Трелле", "Trele"], PpePriceSourceMethod.SupplierCabinet,
            "Щодня о 08:00",
            "Входить у кабінет дилера Трєле, відкриває сторінку кожного товару і бере дилерську ціну без знижки.",
            0m, PpePriceFileKind.None, 0, PpePriceUpdateCadence.Daily, false),
        new(Cemto, "СЕМТО", ["СЕМТО", "CEMTO", "Семто"], PpePriceSourceMethod.File,
            "Вручну, коли приходить новий прайс",
            "PDF-прайс СЕМТО «гурт»: артикул з першої колонки і «Ціна з ПДВ» з того самого рядка. Товар знаходиться за артикулом постачальника.",
            0m, PpePriceFileKind.CemtoPdf, 1, PpePriceUpdateCadence.Manual, false),
    ];

    public static PpePriceSource? Find(string key)
        => All.FirstOrDefault(source => string.Equals(source.Key, key, StringComparison.OrdinalIgnoreCase));
}
