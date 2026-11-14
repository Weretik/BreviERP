using Accounting.Api.Access;
using Accounting.Api.Endpoints.Transactions.CreateFinanceTransaction;
using Accounting.Api.Endpoints.Transactions.GetFinanceTransactionHistory;
using Accounting.Api.Endpoints.Transactions.GetFinanceTransactions;
using Accounting.Api.Endpoints.Transactions.PromoteFinanceOneoff;
using Accounting.Api.Endpoints.Transactions.ReviewFinanceTransaction;
using Accounting.Api.Endpoints.Transactions.UpdateFinanceTransaction;
using Accounting.Api.Endpoints.Transactions.VoidFinanceTransaction;

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
