using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>
/// The manager line under each КП fabric: composition, density, price per metre and supplier (at most 200).
/// Fabrics that no longer exist are missing.
/// </summary>
internal static class GetCrmQuoteFabricFactsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("fabrics", HandleAsync).WithName("getCrmQuoteFabricFacts");

    private static async Task<IResult> HandleAsync([FromQuery] int[] ids, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteFabricFactsQuery(ids), cancellationToken)).ToHttpResult();
}
