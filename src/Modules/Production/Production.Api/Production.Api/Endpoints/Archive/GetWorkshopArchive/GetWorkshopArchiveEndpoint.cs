using Production.Application.Features.Archive.Queries.GetWorkshopArchive;

namespace Production.Api.Endpoints.Archive.GetWorkshopArchive;

/// <param name="kind">Schedule («ГРАФИК работы») or Request (the old requests).</param>
internal static class GetWorkshopArchiveEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopArchive");

    private static async Task<IResult> HandleAsync(string kind, ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, DateOnly? from = null, DateOnly? to = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(
                new GetWorkshopArchiveQuery(actor, kind, search, from, to, page, pageSize),
                    cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
