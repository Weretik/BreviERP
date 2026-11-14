using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Domain.Audit;

namespace Accounting.Api.Endpoints.Transactions.GetFinanceTransactionHistory;

internal static class GetFinanceTransactionHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}/history", HandleAsync).WithName("getFinanceTransactionHistory");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Transaction, id), cancellationToken)).ToHttpResult();
}
