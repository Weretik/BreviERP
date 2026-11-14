using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.GetFinanceCounterpartyHistory;

internal static class GetFinanceCounterpartyHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}/history", HandleAsync).WithName("getFinanceCounterpartyHistory");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Counterparty, id), cancellationToken)).ToHttpResult();
}
