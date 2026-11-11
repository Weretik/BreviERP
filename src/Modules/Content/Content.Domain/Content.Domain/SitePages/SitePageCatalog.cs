namespace Content.Domain.SitePages;

/// <summary>A fixed page of the public site whose search texts staff edit (CNT-007).</summary>
/// <param name="Key">Stable key the site asks for, for example <c>wholesale</c>.</param>
/// <param name="Name">The name staff see.</param>
/// <param name="Path">The address without the language, for example <c>/order-in-bulk</c>.</param>
/// <param name="TitleHasBrand">The title already ends with the brand (the home page); other titles get « — Brevi».</param>
public sealed record SitePageEntry(string Key, string Name, string Path, bool TitleHasBrand = false);

/// <summary>
/// The fixed pages of the public site in menu order. Product, category and article pages have their own SEO in
/// «Товари сайту», «Розділи сайту» and «Статті».
/// </summary>
public static class SitePageCatalog
{
    public static readonly IReadOnlyList<SitePageEntry> Pages =
    [
        new("home", "Головна", "/", TitleHasBrand: true),
        new("catalog", "Каталог", "/catalog"),
        new("logoPrinting", "Нанесення логотипу", "/logo-printing"),
        new("wholesale", "Оптом", "/order-in-bulk"),
        new("industries", "Галузі", "/industries"),
        new("production", "Про виробництво", "/about-company"),
        new("ourWork", "Наші роботи", "/our-work"),
        new("certificates", "Сертифікати", "/certificates"),
        new("articles", "Статті (список)", "/articles"),
        new("regions", "Регіони доставки", "/regions"),
        new("deliveryAndPayment", "Доставка та оплата", "/delivery-and-payment"),
        new("returnsExchanges", "Повернення та обмін", "/returns-exchanges"),
        new("sizeChart", "Таблиця розмірів", "/size-chart"),
        new("contacts", "Контакти", "/contacts"),
        new("agreement", "Договір оферти", "/agreement"),
        new("request", "Заявка (не в пошуку)", "/request"),
    ];

    private static readonly Dictionary<string, SitePageEntry> ByKey =
        Pages.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static SitePageEntry? Find(string key) => ByKey.GetValueOrDefault(key);
}
