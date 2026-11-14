using Accounting.Api.Access;

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
