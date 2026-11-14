using Production.Application.Features.Work.Queries.GetWorkChoices;

namespace Production.Api.Endpoints.Work;

/// <summary>Workers, items of «Нове» orders and over-plan reasons for «Додати роботу».</summary>
internal static class GetWorkChoicesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("work/choices", HandleAsync).WithName("getWorkChoices");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetWorkChoicesQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
