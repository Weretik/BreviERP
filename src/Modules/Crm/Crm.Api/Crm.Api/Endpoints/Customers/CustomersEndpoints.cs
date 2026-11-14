using Crm.Api.Access;

namespace Crm.Api.Endpoints.Customers;

internal static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/customers").RequireAuthorization(CrmAccess.Counterparties)
            .WithTags("Customers");
        GetCrmCustomersEndpoint.Map(group);
        GetCrmCustomerEndpoint.Map(group);
        CreateCrmCustomerEndpoint.Map(group);
        ReviseCrmCustomerEndpoint.Map(group);
        MergeCrmCustomersEndpoint.Map(group);
        SyncCrmCustomersFromKeepinCrmEndpoint.Map(group);
        return app;
    }
}
