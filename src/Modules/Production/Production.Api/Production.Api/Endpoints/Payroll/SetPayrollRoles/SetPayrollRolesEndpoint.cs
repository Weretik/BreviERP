using Production.Application.Features.Payroll.Commands.SetPayrollRoles;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.SetPayrollRoles;

/// <summary>«Керівні частки»: the master and the chief.</summary>
internal static class SetPayrollRolesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{month}/roles", HandleAsync).WithName("setPayrollRoles");

    private static async Task<IResult> HandleAsync(string month, SetPayrollRolesInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SetPayrollRolesCommand(actor, month, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
