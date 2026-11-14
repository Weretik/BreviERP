using Crm.Application.Features.Leads.Queries.GetLeadStats;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadStats;

/// <param name="from">Kyiv date, included; default 6 days before `to`.</param>
/// <param name="to">Kyiv date, included; default today.</param>
internal static class GetCrmLeadStatsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmLeadStats");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, DateOnly? from = null,
        DateOnly? to = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetLeadStatsQuery(from, to), cancellationToken)).ToHttpResult();
}
