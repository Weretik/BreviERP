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
/// </summary>
public sealed record AccessArea(
    string Key,
    string Group,
    string Title,
    AccessAction Actions,
    IReadOnlyDictionary<string, AccessAction> Defaults);

/// <summary>The pages of «Права доступу» in menu order (ACC-RIGHTS, client corrections item 11).</summary>
public static class AccessCatalog
{
    private const AccessAction V = AccessAction.View;
    private const AccessAction VE = AccessAction.View | AccessAction.Edit;
    private const AccessAction VCE = AccessAction.View | AccessAction.Create | AccessAction.Edit;
    private const AccessAction All = AccessAction.View | AccessAction.Create | AccessAction.Edit | AccessAction.Delete;

    public const string Quotes = "quotes";
    public const string QuoteSettings = "quote-settings";
    public const string Embroidery = "embroidery";
    public const string Tasks = "tasks";
    public const string Leads = "leads";
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

    private const string Service = "Менеджер сервісу";
    private const string Workshop = "Швейний цех";
    private const string WarehouseGroup = "Склад тканин";
    private const string Finance = "Бухгалтерія";
    private const string RemindersGroup = "Нагадування";
    private const string References = "Загальні довідники";
    private const string Photos = "Фото";
    private const string SiteContent = "Контент";
    private const string Administration = "Адміністрування";

    private static Dictionary<string, AccessAction> Grant(params (string Role, AccessAction Actions)[] grants) =>
        grants.ToDictionary(x => x.Role, x => x.Actions, StringComparer.Ordinal);

    private static readonly Dictionary<string, AccessAction> Nobody = [];

    public static readonly IReadOnlyList<AccessArea> Areas =
    [
        new(Quotes, Service, "Комерційні пропозиції", All,
            Grant((RoleNames.Admin, VCE), (RoleNames.Manager, VCE))),
        new(QuoteSettings, Service, "Налаштування КП", VE, Grant((RoleNames.Admin, VE))),
        new(Embroidery, Service, "Ціна вишивки", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE))),
        new(Tasks, Service, "Мої завдання", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE))),
        new(Leads, Service, "Ліди", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE))),

        new(ProductionOrders, Workshop, "Виробничі завдання", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.WorkshopChief, All))),
        new(WorkLog, Workshop, "Журнал робіт", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(Forecast, Workshop, "Прогноз пошиття", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopStaff, Workshop, "Працівники цеху", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(FabricNorms, Workshop, "Норми витрати тканини", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopSettings, Workshop, "Налаштування цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopArchive, Workshop, "Архів цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),
        new(Payroll, Workshop, "Зарплата", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(Timesheet, Workshop, "Робочий час", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopReports, Workshop, "Звіти цеху", V, Grant((RoleNames.Admin, V))),

        new(FabricStock, WarehouseGroup, "Залишки, рух і витрата тканини", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),
        new(FabricPlanFact, WarehouseGroup, "План і факт", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.WorkshopChief, V))),
        new(FabricBooking, WarehouseGroup, "Бронь і що замовити", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.WorkshopChief, V))),
        new(FabricValuation, WarehouseGroup, "Вартість складу", V, Grant((RoleNames.Admin, V))),
        new(HardwareOrder, WarehouseGroup, "Фурнітура до замовлення", V,
            Grant((RoleNames.Admin, V), (RoleNames.WorkshopChief, V))),

        new(Accounting, Finance, "Бухгалтерія: облік, перекази, рахунки і каси, звіти", VE, Nobody),
        new(PurchaseInvoices, Finance, "Рахунки на закупку", VE, Nobody),

        new(Reminders, RemindersGroup, "Нагадування", VE, Grant((RoleNames.Admin, VE))),

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

        new(Media, Photos, "Бібліотека фото", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, All))),

        new(ContentProducts, SiteContent, "Товари сайту", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, V))),
        new(ContentCategories, SiteContent, "Розділи сайту", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, V))),
        new(ContentArticles, SiteContent, "Статті", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, V))),

        new(LeadStats, Administration, "Ліди: зведення, довідники, синхронізація", VCE, Grant((RoleNames.Admin, VCE)))
    ];

    private static readonly Dictionary<string, AccessArea> ByKey = Areas.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static AccessArea? Find(string key) => ByKey.GetValueOrDefault(key);

    /// <summary>Roles whose rights cannot be changed: the director may always do everything.</summary>
    public static bool IsLocked(string role) => role == RoleNames.Director;

    /// <summary>Built-in roles: the code knows them, so they are never deleted.</summary>
    public static readonly IReadOnlyList<string> BuiltInRoles =
        [RoleNames.Director, RoleNames.Admin, RoleNames.Manager, RoleNames.Accountant, RoleNames.WorkshopChief];

    public static bool IsBuiltIn(string role) => BuiltInRoles.Contains(role, StringComparer.Ordinal);

    /// <summary>What people see for a built-in role; a new role keeps the name it was given.</summary>
    public static readonly IReadOnlyDictionary<string, string> BuiltInTitles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [RoleNames.Director] = "Директор",
        [RoleNames.Admin] = "Адміністратор",
        [RoleNames.Manager] = "Менеджер",
        [RoleNames.Accountant] = "Бухгалтер",
        [RoleNames.WorkshopChief] = "Начальник цеху"
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
