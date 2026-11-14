using Production.Application.Features.Payroll.Queries.GetPayrollMonth;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Payroll.GetPayrollMonth;

/// <summary>A month <c>YYYY-MM</c>; a closed month as it was closed.</summary>
internal static class GetPayrollMonthEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{month}", HandleAsync).WithName("getPayrollMonth");

    private static async Task<IResult> HandleAsync(string month, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetPayrollMonthQuery(actor, month), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
