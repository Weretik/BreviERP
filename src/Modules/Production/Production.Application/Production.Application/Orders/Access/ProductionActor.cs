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

    public ProductionQuoteReaderActor QuoteReader => new(UserId, IsAdmin);
}
