using Production.Application.Features.Payroll.Shared.Month;
using Production.Application.Features.Payroll.Commands.VoidPayrollPayment;
using Production.Application.Features.Payroll.Shared;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.VoidPayrollPayment;

internal static class VoidPayrollPaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("payments/{paymentId:int}/void", HandleAsync).WithName("voidPayrollPayment")
            .WithSummary("Cancels a payment with a reason (Admin)")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<PayrollMonthResponse>();

    private static async Task<IResult> HandleAsync(int paymentId, VoidPayrollEntryInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new VoidPayrollPaymentCommand(actor, paymentId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
