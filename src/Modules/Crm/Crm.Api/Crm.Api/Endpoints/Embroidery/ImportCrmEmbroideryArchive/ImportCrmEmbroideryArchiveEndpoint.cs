using Crm.Application.Features.Embroidery.Commands.ImportEmbroideryArchive;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Embroidery.ImportCrmEmbroideryArchive;

/// <summary>
/// «Імпорт архіву» (Director and Admin): moves the old calculator's saved calculations; with <c>dryRun</c> only
/// shows what would happen.
/// </summary>
internal static class ImportCrmEmbroideryArchiveEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations/import", HandleAsync).WithName("importCrmEmbroideryArchive")
            .WithSummary("«Імпорт архіву» (Director and Admin)")
            .WithDescription("«Імпорт архіву» (Director and Admin). Moves calculations of the old calculator's archive into «Збережені» with their own dates, authors (matched by email, otherwise the email is shown) and frozen rates. Each record is priced with its old rates and must give the old saved prices to the kopeck, otherwise its line is `error`. A record already moved (same `legacyId`) is `unchanged`. With `dryRun=true`, or when any line is `error`, nothing is saved and `dryRun` in the answer is true.")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .WithMetadata(new RequestSizeLimitAttribute(20_000_000))
            .Produces<EmbroideryArchiveImportResult>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user,
        [FromBody] IReadOnlyList<EmbroideryArchiveRow> rows,
        [FromServices] ISender sender, bool dryRun = default, CancellationToken cancellationToken = default)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new ImportEmbroideryArchiveCommand(actor, rows, dryRun),
            cancellationToken)).ToHttpResult();
    }
}
