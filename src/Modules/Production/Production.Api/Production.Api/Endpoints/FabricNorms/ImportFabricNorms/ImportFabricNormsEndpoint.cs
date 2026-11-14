using Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.FabricNorms.ImportFabricNorms;

/// <summary>Loads norms from a list; with <c>dryRun</c> only shows what would change. If a row is wrong nothing is saved and its line says why.</summary>
internal static class ImportFabricNormsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).WithName("importFabricNorms");

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<FabricNormImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new ImportFabricNormsCommand(rows, dryRun, user.UserId()),
            cancellationToken)).ToHttpResult();
}
