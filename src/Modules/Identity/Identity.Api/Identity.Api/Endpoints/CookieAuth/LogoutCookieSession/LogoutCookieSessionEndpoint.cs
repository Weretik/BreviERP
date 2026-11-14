using Identity.Application.Features.Auth.Commands.CookieLogout;
using Identity.Api.Endpoints.CookieAuth.Shared;

namespace Identity.Api.Endpoints.CookieAuth.LogoutCookieSession;

internal static class LogoutCookieSessionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("logout", HandleAsync).WithName("logoutCookieSession").Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization();

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        [FromServices] SessionCookieWriter cookies, CancellationToken cancellationToken)
    {
        await sender.Send(new CookieLogoutCommand(), cancellationToken);
        cookies.DeleteCsrfToken(response);
        return Results.NoContent();
    }
}
