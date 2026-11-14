using Accounting.Api.Attachments;
using Accounting.Application.Features.Export.Queries.GetFinanceExport;

namespace Accounting.Api.Endpoints.Reports;

internal static class GetFinanceExportEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceExport");

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceExportQuery(), cancellationToken);
        if (!result.IsSuccess) return result.ToFileHttpResult();
        response.Headers.CacheControl = "private, no-store";
        return Results.File(result.Value.Content, "application/json", result.Value.FileName);
    }
}
