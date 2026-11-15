using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Queries.GetLeadCallResults;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadCallResults;

/// <summary>The active call result buttons, in order.</summary>
internal static class GetCrmLeadCallResultsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("call-results", HandleAsync).WithName("getCrmLeadCallResults")
            .WithSummary("Get CRM lead call results")
            .WithDescription("Requires access:leads (view). The active call result buttons, in order.")
            .Produces<IReadOnlyList<LeadCallResultResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetLeadCallResultsQuery(true), cancellationToken)).ToHttpResult();
}
