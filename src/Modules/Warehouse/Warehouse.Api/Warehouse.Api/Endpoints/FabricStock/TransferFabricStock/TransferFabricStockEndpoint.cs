using Warehouse.Application.Features.Stock.Commands.TransferStock;
using Warehouse.Application.Features.Stock.Shared;
using BuildingBlocks.Api.Errors;

namespace Warehouse.Api.Endpoints.FabricStock.TransferFabricStock;

/// <summary>«Перенесення з v133» (Admin): replaces the stock with the D1 export of the old «Цех». <c>dryRun</c> checks and rolls back; <c>confirmReplace</c> allows removing records entered in Brevi itself.</summary>
internal static class TransferFabricStockEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("transfer", HandleAsync).WithName("transferFabricStock")
            .WithSummary("«Перенесення з v133» (Admin) replaces the stock with the D1 export of the old «Цех»")
            .WithDescription("WH-006. The body holds the rows of nine D1 tables (camelCase columns, values as exported). Every link, before + change = after, the chain of every line and the issued metres are checked; truncated values are refused. The stock is emptied and written in one transaction, revisions are replayed, and every movement and line is compared with the export; any difference rolls back. A dry run rolls back too. Records entered in Brevi itself are removed only with confirmReplace. The same export again changes nothing.").Produces<StockTransferResult>()
            .WithMetadata(new RequestSizeLimitAttribute(50_000_000));

    private static async Task<IResult> HandleAsync(StockTransferInput input, ClaimsPrincipal user, [FromServices] ISender sender, bool? dryRun = null, bool? confirmReplace = null, CancellationToken cancellationToken = default) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new TransferStockCommand(actor, input, dryRun ?? false, confirmReplace ?? false), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
