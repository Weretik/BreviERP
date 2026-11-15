using Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Overview.GetFinanceOverview;

internal static class GetFinanceOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceOverview")
            .WithSummary("Get finance overview")
            .WithDescription("«Огляд»: per currency of the active accounts, the balance of the chosen area (both without scope; balances ignore the period) and active income and expense of the inclusive period. For Current, unconfirmedCutoverCount > 0 means the total is not known yet.")
            .Produces<FinanceOverviewResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, DateOnly? from = null,
        DateOnly? to = null, QueryEnum<LedgerScope>? scope = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceOverviewQuery(scope?.Value, from ?? default, to ?? default),
            cancellationToken)).ToHttpResult();
}
