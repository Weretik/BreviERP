using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Counterparties;

/// <summary>
/// «Бухгалтерія → Контрагенти»: finance counterparties of this module, clients of «Клієнти» and suppliers of
/// «Постачальники» (read-only here), and one-off names of operations. 409 on a stale version.
/// </summary>
internal static class FinanceCounterpartiesEndpoints
{
    public static IEndpointRouteBuilder MapFinanceCounterpartiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/counterparties").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceCounterparties");
        GetFinanceCounterpartiesEndpoint.Map(group);
        GetFinanceCounterpartyChoicesEndpoint.Map(group);
        CreateFinanceCounterpartyEndpoint.Map(group);
        UpdateFinanceCounterpartyEndpoint.Map(group);
        VerifyFinanceCounterpartyEndpoint.Map(group);
        ArchiveFinanceCounterpartyEndpoint.Map(group);
        RestoreFinanceCounterpartyEndpoint.Map(group);
        MergeFinanceCounterpartyEndpoint.Map(group);
        GetFinanceCounterpartyHistoryEndpoint.Map(group);
        return app;
    }
}
