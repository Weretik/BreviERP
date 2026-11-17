using BuildingBlocks.Api.Errors;
using Crm.Application.Features.Quotes.Commands.CompleteMailboxConnect;


namespace Crm.Api.Endpoints.Mailboxes.CrmMailboxGoogleCallback;

/// <summary>Google returns the browser here; it always redirects back to the Admin with the outcome.</summary>
internal static class CrmMailboxGoogleCallbackEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("google-callback", HandleAsync).WithName("crmMailboxGoogleCallback")
            .WithSummary("Google OAuth callback, anonymous; the signed state carries the staff member")
            .WithDescription("Google OAuth callback, anonymous; the signed state carries the staff member. Stores the Gmail address and the encrypted refresh token, then redirects to the Admin. Not called by the Admin code.")
            .AllowAnonymous();

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        string? code = null, string? state = null, string? error = null, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CompleteMailboxConnectCommand(code, state, error), cancellationToken);
        response.Headers.CacheControl = "no-store";
        return result.IsSuccess ? Results.Redirect(result.Value) : ApiProblems.BadRequest("Не вдалося підключити пошту. Спробуйте ще раз зі сторінки «Пошта».");
    }
}
