using Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

namespace Content.Api.Endpoints.Redirects.DeleteSiteRedirect;

internal static class DeleteSiteRedirectEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteSiteRedirect");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSiteRedirectCommand(id), cancellationToken)).Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => Results.NoContent(),
            ResultStatus.NotFound => Results.NotFound(),
            ResultStatus.Invalid => Results.BadRequest(),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError),
        };
}
