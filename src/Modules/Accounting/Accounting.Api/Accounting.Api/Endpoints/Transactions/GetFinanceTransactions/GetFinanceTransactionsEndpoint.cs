using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Transactions.GetFinanceTransactions;

internal static class GetFinanceTransactionsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceTransactions")
            .WithSummary("«Операції»: one area, newest first by default (ties: newest saved first)")
            .WithDescription("«Операції»: one area, newest first by default (ties: newest saved first). Totals cover the whole filtered selection per currency, without cancelled rows. needsReview keeps «Потребують перевірки» (verification other than Verified); counterpartyKind with counterpartyId keeps one counterparty.")
            .Produces<FinanceTransactionPageResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<LedgerScope>? scope = null,
        DateOnly? from = null, DateOnly? to = null, Guid? accountId = null, QueryEnum<CategoryDirection>? type = null,
        Guid? categoryId = null, Guid? subcategoryId = null, string? search = null, long? minMinor = null,
        long? maxMinor = null, QueryEnum<MovementSource>? source = null, bool includeVoided = false,
        bool needsReview = false, QueryEnum<CounterpartyKind>? counterpartyKind = null, string? counterpartyId = null,
        QueryEnum<FinanceJournalSort>? sort = null, QueryEnum<FinanceSortDirection>? direction = null, int page = 1,
        CancellationToken cancellationToken = default)
    {
        return (await sender.Send(new GetFinanceTransactionsQuery(
            new FinanceTransactionFilter(scope?.Value ?? LedgerScope.Current, from, to, accountId, type?.Value,
                categoryId, subcategoryId, search, minMinor,
                maxMinor, source?.Value, includeVoided, needsReview, AccountingQueryLink.Of(counterpartyKind?.Value,
                    counterpartyId)), sort?.Value ?? FinanceJournalSort.Date,
                    direction?.Value ?? FinanceSortDirection.Desc, page), cancellationToken)).ToHttpResult();
    }
}
