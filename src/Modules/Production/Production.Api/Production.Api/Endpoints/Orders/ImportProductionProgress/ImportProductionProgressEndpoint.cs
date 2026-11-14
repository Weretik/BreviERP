using Production.Application.Features.Orders.Commands.ImportProductionProgress;
using Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.ImportProductionProgress;

/// <summary>Moves stages, file links and Telegram marks of the old «Цех» (Admin); <c>dryRun</c> only shows them.</summary>
internal static class ImportProductionProgressEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("progress-import", HandleAsync).WithName("importProductionProgress");

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<ProductionProgressImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionProgressCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
