using Production.Application.Features.Payroll.Commands.VoidPayrollPayment;
using Production.Application.Features.Payroll.Shared;

namespace Production.Api.Endpoints.Payroll;

internal static class VoidPayrollPaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("payments/{paymentId:int}/void", HandleAsync).WithName("voidPayrollPayment")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int paymentId, VoidPayrollEntryInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new VoidPayrollPaymentCommand(actor, paymentId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
