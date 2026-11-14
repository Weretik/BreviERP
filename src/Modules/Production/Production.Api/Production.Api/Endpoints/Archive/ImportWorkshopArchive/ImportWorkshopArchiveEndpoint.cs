using Production.Application.Features.Archive.Commands.ImportWorkshopArchive;

namespace Production.Api.Endpoints.Archive.ImportWorkshopArchive;

/// <summary>Moves workshop-archive.json (Admin); <c>dryRun</c> only counts; a line already moved is skipped.</summary>
internal static class ImportWorkshopArchiveEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).WithName("importWorkshopArchive");

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<WorkshopArchiveImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportWorkshopArchiveCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
