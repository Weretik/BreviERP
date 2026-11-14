using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.GetFinanceAccountHistory;

internal static class GetFinanceAccountHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}/history", HandleAsync).WithName("getFinanceAccountHistory");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Account, id), cancellationToken)).ToHttpResult();
}
