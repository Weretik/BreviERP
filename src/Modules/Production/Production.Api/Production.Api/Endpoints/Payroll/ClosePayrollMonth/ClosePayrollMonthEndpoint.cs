using Production.Application.Features.Payroll.Shared.Month;
using Production.Application.Features.Payroll.Commands.ClosePayrollMonth;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.ClosePayrollMonth;

/// <summary>Closes the month: keeps the calculation and locks work, hours and pay in it.</summary>
internal static class ClosePayrollMonthEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{month}/close", HandleAsync).WithName("closePayrollMonth")
            .WithSummary("Closes the month: keeps the calculation and locks work, hours and pay in it (Admin)")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<PayrollMonthResponse>();

    private static async Task<IResult> HandleAsync(string month, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ClosePayrollMonthCommand(actor, month), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
