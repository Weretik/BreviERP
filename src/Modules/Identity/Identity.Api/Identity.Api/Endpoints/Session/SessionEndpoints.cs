namespace Identity.Api.Endpoints.Session;

/// <summary>The signed-in user of the current browser session.</summary>
internal static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth/session").RequireAuthorization().WithTags("AuthSession");
        GetSessionMeEndpoint.Map(group);
        GetSessionAccessEndpoint.Map(group);
        return app;
    }
}
