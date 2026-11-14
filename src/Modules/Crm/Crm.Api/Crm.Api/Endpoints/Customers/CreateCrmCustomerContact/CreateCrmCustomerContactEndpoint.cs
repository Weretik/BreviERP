using Crm.Application.Features.Customers.Commands.SaveCustomerContact;
using Crm.Api.Endpoints.Customers.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.CreateCrmCustomerContact;

internal static class CreateCrmCustomerContactEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmCustomerContact");

    private static async Task<IResult> HandleAsync(Guid customerId, SaveCustomerContactRequest request,
        ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new SaveCustomerContactCommand(customerId, null,
            request.ExpectedRevision, request.Input, request.RequestId, actor), cancellationToken)).ToHttpResult();
    }
}
