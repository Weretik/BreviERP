using Purchasing.Api.Access;

namespace Purchasing.Api.Endpoints.Register;

/// <summary>«Контрагенти → Рахунки на закупку»: the register read from the sheet «финансы».</summary>
internal static class RegisterEndpoints
{
    public static IEndpointRouteBuilder MapRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/purchasing/invoices").RequireAuthorization(PurchasingAccess.Policy)
            .WithTags("PurchaseInvoices");
        GetPurchaseInvoicesEndpoint.Map(group);
        GetPurchaseSyncStateEndpoint.Map(group);
        SyncPurchaseInvoicesEndpoint.Map(group);
        GetPurchaseSuppliersEndpoint.Map(group);
        ReviewPurchaseInvoiceEndpoint.Map(group);
        GetPurchasePaymentCandidatesEndpoint.Map(group);
        LinkPurchasePaymentEndpoint.Map(group);
        UnlinkPurchasePaymentEndpoint.Map(group);
        return app;
    }
}
