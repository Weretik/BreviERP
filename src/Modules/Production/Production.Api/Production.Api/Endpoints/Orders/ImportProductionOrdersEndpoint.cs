using Production.Application.Features.Orders.Commands.ImportProductionOrders;

namespace Production.Api.Endpoints.Orders;

/// <summary>Moves orders of the old «Цех» (Admin); with <c>dryRun</c> only shows what would happen.</summary>
internal static class ImportProductionOrdersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).WithName("importProductionOrders");

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<ProductionOrderImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionOrdersCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
