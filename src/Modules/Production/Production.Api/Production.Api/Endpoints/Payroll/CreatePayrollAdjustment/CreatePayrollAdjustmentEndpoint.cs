using Production.Application.Features.Payroll.Shared.Month;
using Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.CreatePayrollAdjustment;

internal static class CreatePayrollAdjustmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{month}/adjustments", HandleAsync).WithName("createPayrollAdjustment")
            .WithSummary("Adds an extra accrual or a deduction with a reason (Admin)")
            .Produces<PayrollMonthResponse>();

    private static async Task<IResult> HandleAsync(string month, CreatePayrollAdjustmentInput input,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CreatePayrollAdjustmentCommand(actor, month, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
