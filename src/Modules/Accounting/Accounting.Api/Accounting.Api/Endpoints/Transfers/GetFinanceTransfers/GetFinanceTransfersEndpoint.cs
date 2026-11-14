using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Api.Endpoints.Transfers.GetFinanceTransfers;

internal static class GetFinanceTransfersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceTransfers");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<LedgerScope>? scope = null,
        DateOnly? from = null, DateOnly? to = null, Guid? accountId = null, string? search = null,
        long? minMinor = null, long? maxMinor = null, QueryEnum<MovementSource>? source = null,
        bool includeVoided = false, QueryEnum<FinanceJournalSort>? sort = null,
        QueryEnum<FinanceSortDirection>? direction = null, int page = 1,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceTransfersQuery(
            new FinanceTransferFilter(scope?.Value ?? LedgerScope.Current, from, to, accountId, search, minMinor,
                maxMinor, source?.Value, includeVoided), sort?.Value ?? FinanceJournalSort.Date,
            direction?.Value ?? FinanceSortDirection.Desc, page), cancellationToken)).ToHttpResult();
}
