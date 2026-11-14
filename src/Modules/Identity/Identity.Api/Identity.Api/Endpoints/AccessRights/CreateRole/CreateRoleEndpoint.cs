using Identity.Application.Features.Access.Commands.CreateRole;
using Identity.Api.Access;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.AccessRights.CreateRole;

internal static class CreateRoleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("roles", HandleAsync).WithName("createRole").Produces<AccessRoleResponse>();

    private static async Task<IResult> HandleAsync(CreateRoleRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateRoleCommand(request.Title ?? string.Empty), cancellationToken))
            .ToCreatedHttpResult(AccessRoleResponse.From);
}

public sealed record CreateRoleRequest(string Title);
