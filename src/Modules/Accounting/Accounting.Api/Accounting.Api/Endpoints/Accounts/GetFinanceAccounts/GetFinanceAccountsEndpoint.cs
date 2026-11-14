using Accounting.Application.Features.Accounts.Queries.GetFinanceAccounts;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.GetFinanceAccounts;

internal static class GetFinanceAccountsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceAccounts");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceAccountsQuery(includeArchived), cancellationToken)).ToHttpResult();
}
