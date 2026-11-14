using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Transfers;

/// <summary>«Бухгалтерія → Перекази» and the transfers of «Домашня бухгалтерія». 409 on a stale version.</summary>
internal static class FinanceTransfersEndpoints
{
    public static IEndpointRouteBuilder MapFinanceTransfersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/transfers").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceTransfers");
        GetFinanceTransfersEndpoint.Map(group);
        CreateFinanceTransferEndpoint.Map(group);
        UpdateFinanceTransferEndpoint.Map(group);
        VoidFinanceTransferEndpoint.Map(group);
        GetFinanceTransferHistoryEndpoint.Map(group);
        return app;
    }
}
