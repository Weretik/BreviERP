using Production.Application.Features.Payroll.Commands.SetPayrollIssued;

namespace Production.Api.Endpoints.Payroll;

/// <summary>«Видано» of a worker; a null amount clears it.</summary>
internal static class SetPayrollIssuedEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{month}/issued", HandleAsync).WithName("setPayrollIssued");

    private static async Task<IResult> HandleAsync(string month, SetPayrollIssuedInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SetPayrollIssuedCommand(actor, month, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
