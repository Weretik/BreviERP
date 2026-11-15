using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Accounts.Queries.GetFinanceAccounts;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.GetFinanceAccounts;

internal static class GetFinanceAccountsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceAccounts")
            .WithSummary("«Рахунки і каси»: active accounts by order and name; with includeArchived archived ones follow")
            .Produces<IReadOnlyList<FinanceAccountResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceAccountsQuery(includeArchived), cancellationToken)).ToHttpResult();
}
