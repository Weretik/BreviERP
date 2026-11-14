using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.ReviseCustomer;

namespace Crm.Api.Endpoints.Customers.ReviseCrmCustomer;

internal static class ReviseCrmCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{customerId:guid}", HandleAsync).WithName("reviseCrmCustomer");

    private static async Task<IResult> HandleAsync(Guid customerId, ReviseCustomerRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new ReviseCustomerCommand(customerId,
            request.ExpectedRevision, request.Input, request.Active, request.RequestId, actor),
                cancellationToken)).ToHttpResult();
    }
}
