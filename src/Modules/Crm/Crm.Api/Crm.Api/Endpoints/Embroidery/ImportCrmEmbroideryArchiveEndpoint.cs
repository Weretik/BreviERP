using Crm.Application.Features.Embroidery.Commands.ImportEmbroideryArchive;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Api.Endpoints.Embroidery;

/// <summary>
/// «Імпорт архіву» (Director and Admin): moves the old calculator's saved calculations; with <c>dryRun</c> only
/// shows what would happen.
/// </summary>
internal static class ImportCrmEmbroideryArchiveEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("calculations/import", HandleAsync).WithName("importCrmEmbroideryArchive")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .WithMetadata(new RequestSizeLimitAttribute(20_000_000));

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
