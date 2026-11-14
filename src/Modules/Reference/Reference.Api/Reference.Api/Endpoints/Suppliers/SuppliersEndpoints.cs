using Reference.Api.Endpoints.Suppliers.CreateSupplier;
using Reference.Api.Endpoints.Suppliers.DeleteSupplier;
using Reference.Api.Endpoints.Suppliers.GetSuppliers;
using Reference.Api.Endpoints.Suppliers.SyncKeepinCrmSuppliers;
using Reference.Api.Endpoints.Suppliers.UpdateSupplier;
using Reference.Api.Endpoints.Suppliers.UpdateSupplierBreviFields;

namespace Reference.Api.Endpoints.Suppliers;

/// <summary>Suppliers of «Контрагенти»; KeepinCRM suppliers are synced, Brevi fields stay editable.</summary>
internal static class SuppliersEndpoints
{
    public static IEndpointRouteBuilder MapSuppliersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/suppliers").RequireAuthorization("access:counterparties")
            .WithTags("Suppliers");
        GetSuppliersEndpoint.Map(group);
        CreateSupplierEndpoint.Map(group);
        UpdateSupplierEndpoint.Map(group);
        UpdateSupplierBreviFieldsEndpoint.Map(group);
        SyncKeepinCrmSuppliersEndpoint.Map(group);
        DeleteSupplierEndpoint.Map(group);
        return app;
    }
}
