using BuildingBlocks.Api.Errors;
using Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

namespace Content.Api.Endpoints.Redirects.DeleteSiteRedirect;

internal static class DeleteSiteRedirectEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteSiteRedirect")
            .WithSummary("Removes a redirect; the old address then answers 404");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSiteRedirectCommand(id), cancellationToken)).ToNoContentResult();
}
