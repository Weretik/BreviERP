using Crm.Application.Features.Quotes.Commands.CompleteMailboxConnect;

namespace Crm.Api.Endpoints.Quotes.CrmMailboxGoogleCallback;

/// <summary>Google returns the browser here; it always redirects back to the Admin with the outcome.</summary>
internal static class CrmMailboxGoogleCallbackEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("google-callback", HandleAsync).WithName("crmMailboxGoogleCallback")
            .AllowAnonymous();

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        string? code = null, string? state = null, string? error = null, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CompleteMailboxConnectCommand(code, state, error), cancellationToken);
        response.Headers.CacheControl = "no-store";
        return result.IsSuccess ? Results.Redirect(result.Value) : Results.BadRequest();
    }
}
