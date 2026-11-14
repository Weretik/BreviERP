using Accounting.Api.Access;
using Accounting.Api.Endpoints.Accounts.ArchiveFinanceAccount;
using Accounting.Api.Endpoints.Accounts.CreateFinanceAccount;
using Accounting.Api.Endpoints.Accounts.DeleteFinanceAccount;
using Accounting.Api.Endpoints.Accounts.GetFinanceAccountHistory;
using Accounting.Api.Endpoints.Accounts.GetFinanceAccounts;
using Accounting.Api.Endpoints.Accounts.RestoreFinanceAccount;
using Accounting.Api.Endpoints.Accounts.SetFinanceAccountCutover;
using Accounting.Api.Endpoints.Accounts.UpdateFinanceAccount;

namespace Accounting.Api.Endpoints.Accounts;

/// <summary>«Бухгалтерія → Рахунки і каси». 409 on a stale version: someone saved first.</summary>
internal static class FinanceAccountsEndpoints
{
    public static IEndpointRouteBuilder MapFinanceAccountsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/accounts").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceAccounts");
        GetFinanceAccountsEndpoint.Map(group);
        CreateFinanceAccountEndpoint.Map(group);
        UpdateFinanceAccountEndpoint.Map(group);
        SetFinanceAccountCutoverEndpoint.Map(group);
        ArchiveFinanceAccountEndpoint.Map(group);
        RestoreFinanceAccountEndpoint.Map(group);
        DeleteFinanceAccountEndpoint.Map(group);
        GetFinanceAccountHistoryEndpoint.Map(group);
        return app;
    }
}
