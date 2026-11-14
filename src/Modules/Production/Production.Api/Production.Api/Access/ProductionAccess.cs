using System.Globalization;
using System.Security.Claims;
using Ardalis.Result;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Api.Access;

internal static class ProductionAccess
{
    // «Права доступу» pages of «Швейний цех»: the action comes from the HTTP method; the director always passes.
    public const string Orders = "access:production-orders";
    public const string WorkLog = "access:work-log";
    public const string Forecast = "access:forecast";
    public const string Staff = "access:workshop-staff";
    public const string FabricNorms = "access:fabric-norms";
    public const string Settings = "access:workshop-settings";
    public const string Archive = "access:workshop-archive";
    public const string Payroll = "access:payroll";
    public const string Timesheet = "access:timesheet";
    public const string Reports = "access:workshop-reports";

    /// <summary>The person and their roles for the order rules; null without a user id.</summary>
    public static ProductionActor? Actor(this ClaimsPrincipal user) =>
        user.UserId() is { } id and > 0
            ? new ProductionActor(id, user.IsInRole("Admin"), user.IsInRole("Manager"), user.IsInRole("WorkshopChief"))
            : null;

    /// <summary>Id of the signed-in staff user, kept as «who changed it».</summary>
    public static int? UserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;
}
