using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Queries.GetLeadCategories;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadCategories;

internal static class GetCrmLeadCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories", HandleAsync).WithName("getCrmLeadCategories")
            .WithSummary("Get CRM lead categories")
            .WithDescription("Requires access:lead-stats (view). Category codes met in the sheet with their description and the number of leads.")
            .Produces<IReadOnlyList<LeadCategoryResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetLeadCategoriesQuery(), cancellationToken)).ToHttpResult();
}
