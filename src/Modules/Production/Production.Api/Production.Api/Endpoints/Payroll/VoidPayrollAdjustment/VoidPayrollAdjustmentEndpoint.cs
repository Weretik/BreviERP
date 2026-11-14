using Production.Application.Features.Payroll.Commands.VoidPayrollAdjustment;
using Production.Application.Features.Payroll.Shared;

namespace Production.Api.Endpoints.Payroll.VoidPayrollAdjustment;

internal static class VoidPayrollAdjustmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("adjustments/{adjustmentId:int}/void", HandleAsync).WithName("voidPayrollAdjustment")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int adjustmentId, VoidPayrollEntryInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new VoidPayrollAdjustmentCommand(actor, adjustmentId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
