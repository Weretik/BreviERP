using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.SaveQuoteSettings;
using Crm.Application.Features.Quotes.Shared.Settings;

namespace Crm.Api.Endpoints.Quotes.SaveCrmQuoteSettings;

/// <summary>Saves the whole page; 409 when someone saved a newer version.</summary>
internal static class SaveCrmQuoteSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveCrmQuoteSettings");

    private static async Task<IResult> HandleAsync(SaveQuoteSettingsInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SaveQuoteSettingsCommand(actor, input), cancellationToken)).ToHttpResult();
    }
}
