using Production.Application.Features.Payroll.Queries.GetPayrollEmployee;

namespace Production.Api.Endpoints.Payroll.GetPayrollEmployee;

internal static class GetPayrollEmployeeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{month}/employees/{employeeId:int}", HandleAsync).WithName("getPayrollEmployee");

    private static async Task<IResult> HandleAsync(string month, int employeeId, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetPayrollEmployeeQuery(actor, month, employeeId),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
