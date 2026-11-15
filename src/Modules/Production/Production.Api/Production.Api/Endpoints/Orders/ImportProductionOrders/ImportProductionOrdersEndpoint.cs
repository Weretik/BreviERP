using Production.Application.Features.Orders.Commands.ImportProductionOrders;
using Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.ImportProductionOrders;

/// <summary>Moves orders of the old «Цех» (Admin); with <c>dryRun</c> only shows what would happen.</summary>
internal static class ImportProductionOrdersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).WithName("importProductionOrders")
            .WithSummary("Admin: move orders of the old «Цех»; with dryRun only a preview. A list with any wrong order saves nothing")
            .Produces<ProductionOrderImportResult>();

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<ProductionOrderImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionOrdersCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
