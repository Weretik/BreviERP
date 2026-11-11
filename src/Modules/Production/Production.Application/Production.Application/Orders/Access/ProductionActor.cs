using Production.Application.Abstractions;
using Production.Domain.Orders;

namespace Production.Application.Orders.Access;

/// <summary>
/// The signed-in person and what they may do with production orders. Admin (the director also holds Admin) does
/// everything and alone sees КП prices; a Manager creates orders from their own КП and edits their orders but never
/// sees the sewing rate; the head of the workshop sees every order with the rate and edits it, without КП prices.
/// </summary>
public sealed record ProductionActor(int UserId, bool IsAdmin, bool IsManager, bool IsWorkshopChief)
{
    public bool CanCreate => IsAdmin || IsManager;

    public bool SeesSewingRate => IsAdmin || IsWorkshopChief;

    public bool SeesReportPrice => IsAdmin;

    public bool CanDelete => IsAdmin;

    public bool CanEdit(ProductionOrder order) =>
        order is { IsDeleted: false, Status: ProductionOrderStatus.New } &&
        (IsAdmin || IsWorkshopChief || IsManager && order.ManagerUserId == UserId);

    /// <summary>Етапи and shipments: Admin and the head of the workshop on a «Нове» order.</summary>
    public bool CanTrack(ProductionOrder order) =>
        order is { IsDeleted: false, Status: ProductionOrderStatus.New } && (IsAdmin || IsWorkshopChief);

    public bool CanReopen(ProductionOrder order) =>
        IsAdmin && order is { IsDeleted: false } && order.Status != ProductionOrderStatus.New;

    /// <summary>«Журнал робіт» and «Робота» of an item: the people who see the sewing rate (PRD-D37).</summary>
    public bool SeesWork => SeesSewingRate;

    /// <summary>Records and «Сторно» of work: Admin and the head of the workshop on a «Нове» order.</summary>
    public bool CanRecordWork(ProductionOrder order) => CanTrack(order);

    /// <summary>The order of «Прогноз пошиття».</summary>
    public bool CanReorderForecast => IsAdmin || IsWorkshopChief;

    /// <summary>«Робочий час»: Admin and the head of the workshop mark the hours (PRD-D41).</summary>
    public bool CanKeepTime => IsAdmin || IsWorkshopChief;

    /// <summary>«Зарплата»: the calculation, the roles and the embroidery of a month (PRD-D46).</summary>
    public bool SeesPayroll => IsAdmin || IsWorkshopChief;

    /// <summary>Payments, adjustments, «Видано», closing and reopening a month: Admin only (PRD-D46).</summary>
    public bool CanPay => IsAdmin;

    /// <summary>«Звіти цеху»: Admin, the director holds Admin (PRD-D48).</summary>
    public bool SeesReports => IsAdmin;

    /// <summary>«Налаштування цеху».</summary>
    public bool CanManageSettings => IsAdmin || IsWorkshopChief;

    public ProductionQuoteReaderActor QuoteReader => new(UserId, IsAdmin);
}
