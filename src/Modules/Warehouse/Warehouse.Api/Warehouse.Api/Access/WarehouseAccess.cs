using System.Globalization;
using System.Security.Claims;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Access;

internal static class WarehouseAccess
{
    // «Права доступу» pages: the action comes from the HTTP method; the director always passes. The key of «Склад
    // тканин» stays as it was in «Швейний цех», so the rights saved for it keep working.
    public const string FabricStock = "access:fabric-stock";

    /// <summary>The reports of «Склад тканин», one page of «Права доступу» each.</summary>
    public const string FabricPlanFact = "access:fabric-plan-fact";

    public const string FabricBooking = "access:fabric-booking";

    /// <summary>The owner's page: defaults to Admin (the director) only.</summary>
    public const string FabricValuation = "access:fabric-valuation";

    public const string HardwareOrder = "access:hardware-order";

    /// <summary>The «Тканина» tab lives on the order card of «Виробничі завдання».</summary>
    public const string ProductionOrders = "access:production-orders";

    /// <summary>The person and their roles for the stock rules; null without a user id.</summary>
    public static WarehouseActor? WarehouseActor(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var id) && id > 0
            ? new WarehouseActor(id, user.IsInRole("Admin"), user.IsInRole("Manager"), user.IsInRole("WorkshopChief"))
            : null;
}
