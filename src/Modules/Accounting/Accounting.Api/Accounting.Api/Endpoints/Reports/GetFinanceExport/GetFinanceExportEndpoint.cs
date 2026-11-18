using Microsoft.AspNetCore.RateLimiting;
using BuildingBlocks.Api.Errors;
using Accounting.Application.Features.Export.Queries.GetFinanceExport;
using Accounting.Api.Endpoints.Shared;

namespace Accounting.Api.Endpoints.Reports.GetFinanceExport;

internal static class GetFinanceExportEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceExport").RequireRateLimiting("HeavyOperation")
            .WithSummary("Get finance export")
            .WithDescription("«Резервна копія» (stage 8): one JSON file brevi-finance-YYYY-MM-DD.json (Kyiv day), format brevi-erp-finance-v1, with every stored field of accounts, categories, counterparties, transactions and transfers (cancelled ones too), attachment details (not the files), auditEntries, bankStatements and bankStatementLines. Cache-Control private, no-store.")
            .Produces(StatusCodes.Status200OK, contentType: "application/json");

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceExportQuery(), cancellationToken);
        if (!result.IsSuccess) return result.ToHttpResult();
        response.Headers.CacheControl = "private, no-store";
        return Results.File(result.Value.Content, "application/json", result.Value.FileName);
    }
}
