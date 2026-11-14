using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.MergeCustomers;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Customers.MergeCrmCustomers;

internal static class MergeCrmCustomersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{customerId:guid}/merge", HandleAsync).WithName("mergeCrmCustomers")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(Guid customerId, MergeCustomersRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        return actor is null ? Results.Forbid() : (await sender.Send(new MergeCustomersCommand(customerId,
            request.VictimId, request.ExpectedRevision, request.VictimRevision, request.Reason, request.RequestId,
                actor), cancellationToken)).ToHttpResult();
    }
}
