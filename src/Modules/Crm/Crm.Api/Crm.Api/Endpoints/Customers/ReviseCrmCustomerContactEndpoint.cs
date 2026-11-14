using Crm.Api.Customers;
using Crm.Application.Features.Customers.Commands.SaveCustomerContact;

namespace Crm.Api.Endpoints.Customers;

internal static class ReviseCrmCustomerContactEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{contactId:guid}", HandleAsync).WithName("reviseCrmCustomerContact");

    private static async Task<IResult> HandleAsync(Guid customerId, Guid contactId, SaveCustomerContactRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new SaveCustomerContactCommand(customerId,
            contactId,
            request.ExpectedRevision, request.Input, request.RequestId, actor), cancellationToken)).ToHttpResult();
    }
}
