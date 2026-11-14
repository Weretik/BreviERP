using Identity.Application.Features.Auth.Commands.CookieLogin;
using Identity.Api.Auth;

namespace Identity.Api.Endpoints.CookieAuth;

internal static class LoginCookieSessionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("login", HandleAsync).WithName("loginCookieSession").Produces<SessionMeResponse>()
            .AllowAnonymous().RequireRateLimiting("AuthLogin");

    private static async Task<IResult> HandleAsync(SessionLoginRequest request, HttpResponse response,
        [FromServices] ISender sender, [FromServices] SessionCookieWriter cookies, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CookieLoginCommand(request.Email, request.Password), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null) return result.ToHttpResult();

        cookies.AppendCsrfToken(response);
        return Results.Ok(SessionMeResponse.From(result.Value));
    }
}

public sealed record SessionLoginRequest(string Email, string Password);
