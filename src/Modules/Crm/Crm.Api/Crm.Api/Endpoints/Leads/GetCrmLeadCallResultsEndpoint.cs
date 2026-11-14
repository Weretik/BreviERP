using Crm.Application.Features.Leads.Queries.GetLeadCallResults;

namespace Crm.Api.Endpoints.Leads;

/// <summary>The active call result buttons, in order.</summary>
internal static class GetCrmLeadCallResultsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("call-results", HandleAsync).WithName("getCrmLeadCallResults");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetLeadCallResultsQuery(true), cancellationToken)).ToHttpResult();
}
