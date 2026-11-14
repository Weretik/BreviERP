using Production.Application.Features.Payroll.Commands.ReopenPayrollMonth;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.ReopenPayrollMonth;

internal static class ReopenPayrollMonthEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{month}/reopen", HandleAsync).WithName("reopenPayrollMonth")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(string month, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ReopenPayrollMonthCommand(actor, month), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
