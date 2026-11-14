using Crm.Api.Access;

namespace Crm.Api.Endpoints.Customers;

internal static class CustomerContactsEndpoints
{
    public static IEndpointRouteBuilder MapCustomerContactsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/customers/{customerId:guid}/contacts")
            .RequireAuthorization(CrmAccess.Counterparties)
            .WithTags("CustomerContacts");
        CreateCrmCustomerContactEndpoint.Map(group);
        ReviseCrmCustomerContactEndpoint.Map(group);
        return app;
    }
}
