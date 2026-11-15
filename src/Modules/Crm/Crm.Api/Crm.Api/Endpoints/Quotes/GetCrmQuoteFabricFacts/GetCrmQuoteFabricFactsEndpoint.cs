using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteFabricFacts;

/// <summary>
/// The manager line under each КП fabric: composition, density, price per metre and supplier (at most 200).
/// Fabrics that no longer exist are missing.
/// </summary>
internal static class GetCrmQuoteFabricFactsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("fabrics", HandleAsync).WithName("getCrmQuoteFabricFacts")
            .WithSummary("Get CRM quote fabric facts")
            .WithDescription("R-31. The manager line under each КП fabric: composition, density, price per metre and supplier from the current Reference fabric. Not shown in the PDF; unknown ids are left out.")
            .Produces<IReadOnlyList<QuoteFabricFactsResponse>>();

    private static async Task<IResult> HandleAsync([FromQuery] int[] ids, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteFabricFactsQuery(ids), cancellationToken)).ToHttpResult();
}
