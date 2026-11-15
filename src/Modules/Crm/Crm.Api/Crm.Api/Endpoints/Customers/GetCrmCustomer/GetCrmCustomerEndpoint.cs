using Crm.Application.Abstractions.Customers.Views;
using Crm.Api.Access;
using Crm.Application.Features.Customers.Queries.GetCustomer;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.GetCrmCustomer;

internal static class GetCrmCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{customerId:guid}", HandleAsync).WithName("getCrmCustomer")
            .WithSummary("Get CRM customer")
            .WithDescription("Admin or Manager, server-owned actor. Customer owner scope enforced; merge is Admin-only. Mutation requestId replays the same payload; revisions protect existing roots.")
            .Produces<CustomerDetail>();

    private static async Task<IResult> HandleAsync(Guid customerId, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new GetCustomerQuery(customerId, actor),
            cancellationToken)).ToHttpResult();
    }
}
