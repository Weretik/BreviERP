using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Endpoints.Overview;

internal static class GetFinanceOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceOverview");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, DateOnly? from = null,
        DateOnly? to = null, QueryEnum<LedgerScope>? scope = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceOverviewQuery(scope?.Value, from ?? default, to ?? default),
            cancellationToken)).ToHttpResult();
}
