using Identity.Application.Features.Access.Commands.SaveRoleAccess;
using Identity.Api.Access;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.AccessRights.SaveRoleAccess;

internal static class SaveRoleAccessEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("roles/{role}", HandleAsync).WithName("saveRoleAccess").Produces<AccessRoleResponse>();

    private static async Task<IResult> HandleAsync(string role, SaveRoleAccessRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var grants = AccessActionNames.Parse(request.Grants);
        if (grants is null)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Grants)] = ["Actions are view, create, edit and delete."]
            });

        return (await sender.Send(new SaveRoleAccessCommand(role, request.Version, grants), cancellationToken))
            .ToHttpResult(AccessRoleResponse.From);
    }
}

/// <param name="Version">The version the page was loaded with; 409 when someone saved in between.</param>
/// <param name="Grants">Page key → actions (<c>view</c>, <c>create</c>, <c>edit</c>, <c>delete</c>); «view» comes first.</param>
public sealed record SaveRoleAccessRequest(int Version, IReadOnlyDictionary<string, IReadOnlyList<string>> Grants);
