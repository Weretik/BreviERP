using Crm.Api.Customers;
using Crm.Api.Access;
using Crm.Application.Features.Customers.Commands.CreateCustomer;

namespace Crm.Api.Endpoints.Customers;

internal static class CreateCrmCustomerEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmCustomer");

    private static async Task<IResult> HandleAsync(CreateCustomerRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CreateCustomerCommand(request.Input, request.RequestId, actor),
            cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("getCrmCustomer", new { customerId = result.Value.PublicId }, result.Value)
            : result.ToHttpResult();
    }
}
