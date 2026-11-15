using Production.Application.Features.Payroll.Shared.Month;
using Production.Application.Features.Payroll.Commands.ReopenPayrollMonth;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.ReopenPayrollMonth;

internal static class ReopenPayrollMonthEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{month}/reopen", HandleAsync).WithName("reopenPayrollMonth")
            .WithSummary("Opens a closed month again (Admin)")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<PayrollMonthResponse>();

    private static async Task<IResult> HandleAsync(string month, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ReopenPayrollMonthCommand(actor, month), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
