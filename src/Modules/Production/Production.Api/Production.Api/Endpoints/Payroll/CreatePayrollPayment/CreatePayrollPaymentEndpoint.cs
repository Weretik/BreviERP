using Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

namespace Production.Api.Endpoints.Payroll.CreatePayrollPayment;

internal static class CreatePayrollPaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{month}/payments", HandleAsync).WithName("createPayrollPayment");

    private static async Task<IResult> HandleAsync(string month, CreatePayrollPaymentInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CreatePayrollPaymentCommand(actor, month, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
