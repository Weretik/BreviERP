using Identity.Domain.Authorization.Roles;

namespace Identity.Domain.Authorization;

/// <summary>What a role may do on a page of «Права доступу».</summary>
[Flags]
public enum AccessAction
{
    None = 0,
    View = 1,
    Create = 2,
    Edit = 4,
    Delete = 8
}

/// <summary>
/// A page (area) of the admin whose API is guarded by «Права доступу». <paramref name="Defaults"/> are the rights the built-in
/// roles had before the page existed (the admin menu they saw), so nothing changes until someone saves other rights.
/// With <paramref name="DefaultsForSavedRoles"/> a role saved before the page existed also gets the page's defaults, and a
/// save keeps the page even when it is unticked, so the director's choice wins from then on (DESK-001).
/// </summary>
public sealed record AccessArea(
    string Key,
    string Group,
    string Title,
    AccessAction Actions,
    IReadOnlyDictionary<string, AccessAction> Defaults,
    bool DefaultsForSavedRoles = false);

/// <summary>
/// The pages of «Права доступу» in the groups and order of the admin menu (ACC-RIGHTS, client corrections item 11).
/// A page whose menu item moves moves here too; a page that gains «Створення» or «Видалення» needs a data migration
/// that gives those actions to every saved role that had «Редагування», so nobody loses what they could do.
/// </summary>
public static class AccessCatalog
{
    private const AccessAction V = AccessAction.View;
    private const AccessAction VE = AccessAction.View | AccessAction.Edit;
    private const AccessAction VCE = AccessAction.View | AccessAction.Create | AccessAction.Edit;
    private const AccessAction All = AccessAction.View | AccessAction.Create | AccessAction.Edit | AccessAction.Delete;
    private const AccessAction VC = AccessAction.View | AccessAction.Create;

    public const string DeskMain = "desk-main";
    public const string DeskSales = "desk-sales";
    public const string DeskWorkshop = "desk-workshop";
    public const string DeskSeo = "desk-seo";
    public const string DeskAccounting = "desk-accounting";
    public const string Quotes = "quotes";
    public const string QuoteSettings = "quote-settings";
    public const string Embroidery = "embroidery";
    public const string Tasks = "tasks";
    public const string Leads = "leads";
    public const string SiteRequests = "site-requests";
    public const string ProductionOrders = "production-orders";
    public const string FabricStock = "fabric-stock";
    public const string FabricPlanFact = "fabric-plan-fact";
    public const string FabricBooking = "fabric-booking";
    public const string FabricValuation = "fabric-valuation";
    public const string HardwareOrder = "hardware-order";
    public const string WorkLog = "work-log";
    public const string Forecast = "forecast";
    public const string WorkshopStaff = "workshop-staff";
    public const string FabricNorms = "fabric-norms";
    public const string WorkshopSettings = "workshop-settings";
    public const string WorkshopArchive = "workshop-archive";
    public const string Payroll = "payroll";
    public const string Timesheet = "timesheet";
    public const string WorkshopReports = "workshop-reports";
    public const string Accounting = "accounting";
    public const string PurchaseInvoices = "purchase-invoices";
    public const string Reminders = "reminders";
    public const string Products = "products";
    public const string ProductSections = "product-sections";
    public const string Materials = "materials";
    public const string Operations = "operations";
    public const string Counterparties = "counterparties";
    public const string PricingParameters = "pricing-parameters";
    public const string PriceImport = "price-import";
    public const string Media = "media";
    public const string LeadStats = "lead-stats";
    public const string ContentProducts = "content-products";
    public const string ContentCategories = "content-categories";
    public const string ContentArticles = "content-articles";
    public const string ContentRedirects = "content-redirects";
    public const string ContentSeoSettings = "content-seo-settings";
    public const string ContentSitePages = "content-site-pages";
    public const string ContentSeoOverview = "content-seo-overview";
    public const string ContentCompany = "content-company";
    public const string ContentWorks = "content-works";
    public const string ContentCertificates = "content-certificates";

    private const string Desks = "Робочі столи";
    private const string Service = "Менеджер сервісу";
    private const string Workshop = "Швейний цех";
    private const string WarehouseGroup = "Склад тканин";
    private const string Finance = "Бухгалтерія";
    private const string RemindersGroup = "Нагадування";
    private const string References = "Загальні довідники";
    private const string Photos = "Фото";
    private const string SiteContent = "Контент";
    private const string ReportsGroup = "Загальні звіти";
    private const string Administration = "Адміністрування";

    private static Dictionary<string, AccessAction> Grant(params (string Role, AccessAction Actions)[] grants) =>
        grants.ToDictionary(x => x.Role, x => x.Actions, StringComparer.Ordinal);


    public static readonly IReadOnlyList<AccessArea> Areas =
    [
        // «Робочий стіл» (DESK-001): which desks a role sees on the admin home; each block also needs its own page. Roles saved
        // before the desks existed get their defaults, so nobody loses the home page on deploy.
        new(DeskMain, Desks, "Головний стіл директора", V, Grant((RoleNames.Admin, V)), DefaultsForSavedRoles: true),
        new(DeskSales, Desks, "Стіл менеджера", V, Grant((RoleNames.Admin, V), (RoleNames.Manager, V)), DefaultsForSavedRoles: true),
        new(DeskWorkshop, Desks, "Стіл цеху", V, Grant((RoleNames.Admin, V), (RoleNames.WorkshopChief, V)), DefaultsForSavedRoles: true),
        new(DeskSeo, Desks, "Стіл SEO", V, Grant((RoleNames.Admin, V), (RoleNames.Seo, V)), DefaultsForSavedRoles: true),
        new(DeskAccounting, Desks, "Стіл бухгалтера", V, Grant((RoleNames.Accountant, V)), DefaultsForSavedRoles: true),

        // Groups and order follow the admin menu (legacy-menu.ts), so each page sits where people find it in the menu.
        new(Leads, Service, "Ліди", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE))),
        new(SiteRequests, Service, "Заявки з сайту", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE))),
        new(Quotes, Service, "Комерційні пропозиції", All,
            Grant((RoleNames.Admin, VCE), (RoleNames.Manager, VCE))),
        new(Embroidery, Service, "Ціна вишивки", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, All))),
        new(Tasks, Service, "Мої завдання", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, All))),

        new(ProductionOrders, Workshop, "Виробничі завдання", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.WorkshopChief, All))),
        new(WorkLog, Workshop, "Журнал робіт", VCE, Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE))),
        new(Forecast, Workshop, "Прогноз пошиття", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopStaff, Workshop, "Персонал цеху: працівники", VCE,
            Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE))),
        new(Timesheet, Workshop, "Персонал цеху: робочий час", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(Payroll, Workshop, "Персонал цеху: зарплата", VCE, Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE))),
        new(FabricNorms, Workshop, "Норми витрати тканини", All,
            Grant((RoleNames.Admin, All), (RoleNames.WorkshopChief, All))),
        new(WorkshopArchive, Workshop, "Архів цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),

        new(FabricStock, WarehouseGroup, "Залишки, рух тканини і витрата за період", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.WorkshopChief, All))),
        new(FabricBooking, WarehouseGroup, "Що замовити: тканина (бронь)", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.WorkshopChief, V))),
        new(HardwareOrder, WarehouseGroup, "Що замовити: фурнітура", V,
            Grant((RoleNames.Admin, V), (RoleNames.WorkshopChief, V))),

        new(Accounting, Finance, "Бухгалтерія: облік, перекази, рахунки і каси, звіти", All,
            Grant((RoleNames.Accountant, All)), DefaultsForSavedRoles: true),
        new(PurchaseInvoices, Finance, "Рахунки на закупку", VE, Grant((RoleNames.Accountant, VE)),
            DefaultsForSavedRoles: true),

        new(Reminders, RemindersGroup, "Нагадування: задачі, періоди, чати, неробочі дні", All, Grant((RoleNames.Admin, All))),

        new(Products, References, "Товари", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),
        new(ProductSections, References, "Типи і розділи", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),
        new(Materials, References, "Тканини та фурнітура", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),
        new(Operations, References, "Операції", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),
        new(Counterparties, References, "Контрагенти", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All))),
        new(PricingParameters, References, "Параметри ціноутворення", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),
        new(PriceImport, References, "Імпорт цін", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V))),

        new(Media, Photos, "Бібліотека фото", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Seo, VC))),

        new(ContentProducts, SiteContent, "Товари сайту", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, V), (RoleNames.Seo, VCE))),
        new(ContentCategories, SiteContent, "Розділи сайту", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, V), (RoleNames.Seo, VCE))),
        new(ContentArticles, SiteContent, "Статті", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, V), (RoleNames.Seo, VCE))),
        new(ContentRedirects, SiteContent, "Переадресації", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, V), (RoleNames.Seo, VCE))),
        new(ContentSeoSettings, SiteContent, "Налаштування SEO сайту", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, V), (RoleNames.Seo, VE))),
        new(ContentSitePages, SiteContent, "Сторінки сайту", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, V), (RoleNames.Seo, VE))),
        new(ContentSeoOverview, SiteContent, "SEO-огляд", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.Seo, V))),
        new(ContentCompany, SiteContent, "Контакти та компанія", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, V), (RoleNames.Seo, VE))),
        new(ContentWorks, SiteContent, "Наші роботи", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, V), (RoleNames.Seo, VE))),
        new(ContentCertificates, SiteContent, "Сертифікати", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, V), (RoleNames.Seo, VE))),

        new(WorkshopReports, ReportsGroup, "Звіти цеху", V, Grant((RoleNames.Admin, V))),
        new(FabricPlanFact, ReportsGroup, "Звіти складу: план і факт", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.WorkshopChief, V))),
        new(FabricValuation, ReportsGroup, "Звіти складу: вартість", V, Grant((RoleNames.Admin, V))),

        new(LeadStats, Administration, "Статистика лідів", VCE, Grant((RoleNames.Admin, VCE))),
        new(QuoteSettings, Administration, "Налаштування КП", VE, Grant((RoleNames.Admin, VE))),
        new(WorkshopSettings, Administration, "Налаштування цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE)))
    ];

    private static readonly Dictionary<string, AccessArea> ByKey = Areas.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static AccessArea? Find(string key) => ByKey.GetValueOrDefault(key);

    /// <summary>Roles whose rights cannot be changed: the director may always do everything.</summary>
    public static bool IsLocked(string role) => role == RoleNames.Director;

    /// <summary>Built-in roles: the code knows them, so they are never deleted.</summary>
    public static readonly IReadOnlyList<string> BuiltInRoles =
        [RoleNames.Director, RoleNames.Admin, RoleNames.Manager, RoleNames.Accountant, RoleNames.WorkshopChief, RoleNames.Seo];

    public static bool IsBuiltIn(string role) => BuiltInRoles.Contains(role, StringComparer.Ordinal);

    /// <summary>What people see for a built-in role; a new role keeps the name it was given.</summary>
    public static readonly IReadOnlyDictionary<string, string> BuiltInTitles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [RoleNames.Director] = "Директор",
        [RoleNames.Admin] = "Адміністратор",
        [RoleNames.Manager] = "Менеджер",
        [RoleNames.Accountant] = "Бухгалтер",
        [RoleNames.WorkshopChief] = "Начальник цеху",
        [RoleNames.Seo] = "SEO-фахівець"
    };

    /// <summary>Roles that never reach the admin: shoppers and guests.</summary>
    public static bool IsOutsider(string role) => role is RoleNames.User or RoleNames.Guest;

    /// <summary>Rights of several roles together: an action allowed to any of them is allowed.</summary>
    public static IReadOnlyDictionary<string, AccessAction> Combine(IEnumerable<IReadOnlyDictionary<string, AccessAction>> grants)
    {
        var result = new Dictionary<string, AccessAction>(StringComparer.Ordinal);
        foreach (var (area, actions) in grants.SelectMany(x => x))
        {
            result[area] = result.GetValueOrDefault(area) | actions;
        }

        return result;
    }

    /// <summary>Every page with every action it has: the director's rights.</summary>
    public static IReadOnlyDictionary<string, AccessAction> Everything() =>
        Areas.ToDictionary(x => x.Key, x => x.Actions, StringComparer.Ordinal);

    /// <summary>Rights a role has before anyone saves its page: the old built-in rules, nothing for a new role.</summary>
    public static IReadOnlyDictionary<string, AccessAction> DefaultsOf(string role) =>
        Areas.Select(area => (area.Key, Actions: area.Defaults.GetValueOrDefault(role)))
            .Where(x => x.Actions != AccessAction.None)
            .ToDictionary(x => x.Key, x => x.Actions, StringComparer.Ordinal);

    /// <summary>
    /// The action a request needs when its endpoint names only the page: GET reads, POST creates, PUT and PATCH edit,
    /// DELETE deletes. A page without that action asks for «Редагування», and one without it for «Перегляд».
    /// </summary>
    public static AccessAction Required(AccessArea area, string method, AccessAction? explicitAction)
    {
        var wanted = explicitAction ?? method.ToUpperInvariant() switch
        {
            "GET" or "HEAD" or "OPTIONS" => AccessAction.View,
            "POST" => AccessAction.Create,
            "DELETE" => AccessAction.Delete,
            _ => AccessAction.Edit
        };
        if (area.Actions.HasFlag(wanted)) return wanted;
        return wanted != AccessAction.View && area.Actions.HasFlag(AccessAction.Edit) ? AccessAction.Edit : AccessAction.View;
    }
}
