using Production.Application.Features.Orders.Commands.ImportProductionReportPrices;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;

namespace Production.Api.Endpoints.Orders.ImportProductionReportPrices;

/// <summary>«Ціни КП» of orders moved from the old «Цех» for «Звіти цеху» (Admin); <c>dryRun</c> only shows them.</summary>
internal static class ImportProductionReportPricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("report-price-import", HandleAsync).WithName("importProductionReportPrices");

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<ProductionReportPriceImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionReportPricesCommand(actor, rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
