using Crm.Application.Abstractions.Customers.Views;
using Crm.Application.Features.Customers.Commands.SaveCustomerContact;
using Crm.Api.Endpoints.Customers.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.ReviseCrmCustomerContact;

internal static class ReviseCrmCustomerContactEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{contactId:guid}", HandleAsync).WithName("reviseCrmCustomerContact")
            .WithSummary("Revise CRM customer contact")
            .WithDescription("Admin or Manager, server-owned actor. Customer owner scope enforced; merge is Admin-only. Mutation requestId replays the same payload; revisions protect existing roots.")
            .Produces<CustomerDetail>();

    private static async Task<IResult> HandleAsync(Guid customerId, Guid contactId, SaveCustomerContactRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new SaveCustomerContactCommand(customerId,
            contactId,
            request.ExpectedRevision, request.Input, request.RequestId, actor), cancellationToken)).ToHttpResult();
    }
}
