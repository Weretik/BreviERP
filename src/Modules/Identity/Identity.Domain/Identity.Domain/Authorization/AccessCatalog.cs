using Identity.Domain.Authorization.Roles;

namespace Identity.Domain.Authorization;

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
    public const string EmbroiderySettings = "embroidery-settings";
    public const string Tasks = "tasks";
    public const string ManagerTasks = "manager-tasks";
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
    public const string BackgroundTasks = "background-tasks";
    public const string ActivityLog = "activity-log";
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

    private static readonly AccessActionLabels Desk = new(View: "Бачить блок на головній");

    private static Dictionary<string, AccessAction> Grant(params (string Role, AccessAction Actions)[] grants) =>
        grants.ToDictionary(x => x.Role, x => x.Actions, StringComparer.Ordinal);


    public static readonly IReadOnlyList<AccessArea> Areas =
    [
        // «Робочий стіл» (DESK-001): which desks a role sees on the admin home; each block also needs its own page. Roles saved
        // before the desks existed get their defaults, so nobody loses the home page on deploy.
        new(DeskMain, Desks, "Головний стіл директора", V, Grant((RoleNames.Admin, V)), DefaultsForSavedRoles: true, Labels: Desk),
        new(DeskSales, Desks, "Стіл менеджера", V, Grant((RoleNames.Admin, V), (RoleNames.Manager, V)), DefaultsForSavedRoles: true, Labels: Desk),
        new(DeskWorkshop, Desks, "Стіл цеху", V, Grant((RoleNames.Admin, V), (RoleNames.WorkshopChief, V)), DefaultsForSavedRoles: true, Labels: Desk),
        new(DeskSeo, Desks, "Стіл SEO", V, Grant((RoleNames.Admin, V), (RoleNames.Seo, V)), DefaultsForSavedRoles: true, Labels: Desk),
        new(DeskAccounting, Desks, "Стіл бухгалтера", V, Grant((RoleNames.Accountant, V)), DefaultsForSavedRoles: true, Labels: Desk),

        // Groups and order follow the admin menu (legacy-menu.ts), so each page sits where people find it in the menu.
        new(Leads, Service, "Ліди", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE)),
            Labels: new(Edit: "Працювати з лідами")),
        new(SiteRequests, Service, "Заявки з сайту", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE)),
            Labels: new(Edit: "Брати в роботу і закривати заявки")),
        new(Quotes, Service, "Комерційні пропозиції", All,
            Grant((RoleNames.Admin, VCE), (RoleNames.Manager, VCE)),
            Labels: new(Create: "Створювати і копіювати КП", Edit: "Змінювати, перераховувати і надсилати КП",
                Delete: "Видаляти КП і чистити старий архів")),
        new(Embroidery, Service, "Ціна вишивки", All, Grant((RoleNames.Admin, All), (RoleNames.Manager, All)),
            Labels: new(View: "Бачить і рахує ціну", Create: "Зберігати розрахунки", Edit: "Додавати макет до розрахунку",
                Delete: "Видаляти збережені розрахунки")),
        // ACC-002 R-02: cost, rates and «Налаштування» were the Admin role; a role saved earlier keeps them.
        new(EmbroiderySettings, Service, "Ціна вишивки: собівартість і налаштування", VE, Grant((RoleNames.Admin, VE)),
            DefaultsForSavedRoles: true,
            Labels: new(View: "Бачить собівартість і ставки", Edit: "Змінювати налаштування вишивки")),
        // ACC-002 R-01: giving, changing and deleting tasks is «Завдання менеджерам» in «Адміністрування».
        new(Tasks, Service, "Мої завдання", VE, Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE)),
            Labels: new(View: "Бачить свої завдання", Edit: "Змінювати статус і відмічати кроки")),

        new(ProductionOrders, Workshop, "Виробничі завдання", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.WorkshopChief, All)),
            Labels: new(Create: "Створювати завдання", Edit: "Змінювати, закривати, скасовувати, файли і Telegram",
                Delete: "Видаляти завдання")),
        new(WorkLog, Workshop, "Журнал робіт", VCE, Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE)),
            Labels: new(Create: "Додавати записи робіт", Edit: "Виправляти і анулювати записи")),
        new(Forecast, Workshop, "Прогноз пошиття", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),
        new(WorkshopStaff, Workshop, "Персонал цеху: працівники", VCE,
            Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE)),
            Labels: new(Create: "Додавати працівників", Edit: "Змінювати працівників")),
        new(Timesheet, Workshop, "Персонал цеху: робочий час", VE, Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE))),
        new(Payroll, Workshop, "Персонал цеху: зарплата", VCE, Grant((RoleNames.Admin, VCE), (RoleNames.WorkshopChief, VCE)),
            Labels: new(Create: "Виплати і коригування", Edit: "Анулювати і закривати місяць")),
        new(FabricNorms, Workshop, "Норми витрати тканини", All,
            Grant((RoleNames.Admin, All), (RoleNames.WorkshopChief, All)),
            Labels: new(Delete: "Видаляти норму")),
        new(WorkshopArchive, Workshop, "Архів цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.Manager, VE), (RoleNames.WorkshopChief, VE))),

        new(FabricStock, WarehouseGroup, "Залишки, рух тканини і витрата за період", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.WorkshopChief, All)),
            Labels: new(Create: "Прихід, видача, залишки на початок, коригування", Edit: "Сторно, анулювання, відновлення",
                Delete: "Видаляти рядок")),
        new(FabricBooking, WarehouseGroup, "Що замовити: тканина (бронь)", V,
            Grant((RoleNames.Admin, V), (RoleNames.Manager, V), (RoleNames.WorkshopChief, V))),
        new(HardwareOrder, WarehouseGroup, "Що замовити: фурнітура", V,
            Grant((RoleNames.Admin, V), (RoleNames.WorkshopChief, V))),

        new(Accounting, Finance, "Бухгалтерія: облік, перекази, рахунки і каси, звіти", All,
            Grant((RoleNames.Accountant, All)), DefaultsForSavedRoles: true,
            Labels: new(Create: "Нові операції, перекази, рахунки, категорії, виписки",
                Edit: "Змінювати, анулювати, архівувати, об'єднувати, проводити",
                Delete: "Видаляти рахунки, категорії і вкладення")),
        new(PurchaseInvoices, Finance, "Рахунки на закупку", VE, Grant((RoleNames.Accountant, VE)),
            DefaultsForSavedRoles: true),

        new(Reminders, RemindersGroup, "Нагадування: задачі, періоди, чати, неробочі дні", All, Grant((RoleNames.Admin, All)),
            Labels: new(Create: "Нові задачі і неробочі дні", Edit: "Закривати, відкривати, перевіряти чат",
                Delete: "Архівувати задачі, прибирати дні")),

        new(Products, References, "Товари", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V)),
            Labels: new(Edit: "Редагувати товари, ціни і фото")),
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
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Accountant, V)),
            Labels: new(Create: "Завантажувати і оновлювати ціни", Edit: "Знижки і постачальники джерел")),

        new(Media, Photos, "Бібліотека фото", All,
            Grant((RoleNames.Admin, All), (RoleNames.Manager, All), (RoleNames.Seo, VC)),
            Labels: new(Create: "Завантажувати фото і створювати папки")),

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

        // In the order of the menu group «Адміністрування»; «Користувачі», «Налаштування ШІ» and «Права доступу» are AdminOnly.
        // ACC-002 R-01: «Перегляд» also means seeing every manager's tasks; the reads stay on «Мої завдання».
        new(ManagerTasks, Administration, "Завдання менеджерам", All, Grant((RoleNames.Admin, All)), DefaultsForSavedRoles: true,
            Labels: new(View: "Бачить завдання всіх менеджерів", Create: "Давати завдання",
                Edit: "Змінювати завдання і «Терміново»", Delete: "Видаляти завдання")),
        new(LeadStats, Administration, "Статистика лідів", VCE, Grant((RoleNames.Admin, VCE)),
            Labels: new(Create: "Синхронізувати зараз", Edit: "Змінювати довідники")),
        // SCH-001 R-11: Create = «Запустити зараз», Edit = schedule and on/off.
        new(BackgroundTasks, Administration, "Фонові задачі", VCE, Grant((RoleNames.Admin, VCE)),
            Labels: new(Create: "Запускати зараз", Edit: "Змінювати розклад, вмикати і вимикати")),
        // ACT-001 R-10: view only; entries come from the outbox, nobody edits them.
        new(ActivityLog, Administration, "Журнал дій", V, Grant((RoleNames.Admin, V))),
        new(QuoteSettings, Administration, "Налаштування КП", VE, Grant((RoleNames.Admin, VE))),
        new(WorkshopSettings, Administration, "Налаштування цеху", VE,
            Grant((RoleNames.Admin, VE), (RoleNames.WorkshopChief, VE)))
    ];

    /// <summary>
    /// What only Admin (and the director) may use, on purpose (ACC-002 R-04): system pages and one-time or «бачить усі»
    /// actions. «Права доступу» shows them without switches.
    /// </summary>
    public static readonly IReadOnlyList<AdminOnlyItem> AdminOnly =
    [
        new(Administration, "Користувачі", "Облікові записи і ролі співробітників"),
        new(Administration, "Права доступу", "Ця сторінка"),
        new(Administration, "Налаштування ШІ", "Ключі і моделі ШІ"),
        new(Service, "Передати КП іншому менеджеру", "Потрібно бачити всі КП"),
        new(References, "Об'єднати двох клієнтів", "Потрібно бачити всіх клієнтів"),
        new(Service, "Перенесення старих архівів КП і вишивки", "Одноразове перенесення даних"),
        new(References, "Перенесення фото зі старого сайту", "Одноразове перенесення даних")
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
