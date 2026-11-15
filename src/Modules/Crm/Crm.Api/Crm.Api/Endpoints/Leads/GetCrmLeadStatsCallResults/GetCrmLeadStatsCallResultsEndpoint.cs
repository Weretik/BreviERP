using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Queries.GetLeadCallResults;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadStatsCallResults;

/// <summary>All call results, switched-off ones included.</summary>
internal static class GetCrmLeadStatsCallResultsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("call-results", HandleAsync).WithName("getCrmLeadStatsCallResults")
            .WithSummary("Get CRM lead stats call results")
            .WithDescription("Requires access:lead-stats (view). Every call result, switched-off ones included, in order.")
            .Produces<IReadOnlyList<LeadCallResultResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetLeadCallResultsQuery(false), cancellationToken)).ToHttpResult();
}
