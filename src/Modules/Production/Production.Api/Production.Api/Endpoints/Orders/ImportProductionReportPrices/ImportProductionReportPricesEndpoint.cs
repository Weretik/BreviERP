using Production.Application.Features.Orders.Commands.ImportProductionReportPrices;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.ImportProductionReportPrices;

/// <summary>«Ціни КП» of orders moved from the old «Цех» for «Звіти цеху» (Admin); <c>dryRun</c> only shows them.</summary>
internal static class ImportProductionReportPricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("report-price-import", HandleAsync).WithName("importProductionReportPrices")
            .WithSummary("Admin: «Ціни КП» of orders moved from the old «Цех» for «Звіти цеху»; only an item without a price takes one; dryRun only shows them; any error saves nothing")
            .Produces<ProductionReportPriceImportResult>();

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<ProductionReportPriceImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionReportPricesCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
