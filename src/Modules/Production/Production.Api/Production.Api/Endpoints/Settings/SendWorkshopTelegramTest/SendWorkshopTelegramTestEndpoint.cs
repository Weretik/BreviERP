using Production.Application.Features.Settings.Commands.SendWorkshopTelegramTest;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Settings.SendWorkshopTelegramTest;

/// <summary>Sends a test line to the chosen group and topic.</summary>
internal static class SendWorkshopTelegramTestEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("telegram-test", HandleAsync).WithName("sendWorkshopTelegramTest");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new SendWorkshopTelegramTestCommand(actor), cancellationToken)).ToNoContentHttpResult()
            : Results.Forbid();
}
