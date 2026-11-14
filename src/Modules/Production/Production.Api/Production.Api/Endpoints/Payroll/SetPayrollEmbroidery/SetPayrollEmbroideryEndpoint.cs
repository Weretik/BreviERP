using Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.SetPayrollEmbroidery;

internal static class SetPayrollEmbroideryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{month}/embroidery", HandleAsync).WithName("setPayrollEmbroidery");

    private static async Task<IResult> HandleAsync(string month, SetPayrollEmbroideryInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SetPayrollEmbroideryCommand(actor, month, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
