using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Transactions;

/// <summary>
/// «Бухгалтерія → Операції», «Потребують перевірки» (<c>needsReview</c>) and the operations of «Домашня бухгалтерія».
/// 409 on a stale version.
/// </summary>
internal static class FinanceTransactionsEndpoints
{
    public static IEndpointRouteBuilder MapFinanceTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/transactions").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceTransactions");
        GetFinanceTransactionsEndpoint.Map(group);
        CreateFinanceTransactionEndpoint.Map(group);
        UpdateFinanceTransactionEndpoint.Map(group);
        VoidFinanceTransactionEndpoint.Map(group);
        ReviewFinanceTransactionEndpoint.Map(group);
        PromoteFinanceOneoffEndpoint.Map(group);
        GetFinanceTransactionHistoryEndpoint.Map(group);
        return app;
    }
}
