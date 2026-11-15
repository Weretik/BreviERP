using Crm.Application.Abstractions.Customers.Views;
using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.ReviseCustomer;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.ReviseCrmCustomer;

internal static class ReviseCrmCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{customerId:guid}", HandleAsync).WithName("reviseCrmCustomer")
            .WithSummary("Revise CRM customer")
            .WithDescription("Admin or Manager, server-owned actor. Customer owner scope enforced; merge is Admin-only. Mutation requestId replays the same payload; revisions protect existing roots.")
            .Produces<CustomerDetail>();

    private static async Task<IResult> HandleAsync(Guid customerId, ReviseCustomerRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new ReviseCustomerCommand(customerId,
            request.ExpectedRevision, request.Input, request.Active, request.RequestId, actor),
                cancellationToken)).ToHttpResult();
    }
}
