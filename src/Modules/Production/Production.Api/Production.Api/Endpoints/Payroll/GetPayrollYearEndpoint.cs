using Production.Application.Features.Payroll.Queries.GetPayrollYear;

namespace Production.Api.Endpoints.Payroll;

/// <summary>«Історія за рік».</summary>
internal static class GetPayrollYearEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("history/{year:int}", HandleAsync).WithName("getPayrollYear");

    private static async Task<IResult> HandleAsync(int year, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetPayrollYearQuery(actor, year), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
