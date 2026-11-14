using Crm.Application.Features.Leads.Queries.GetLeadCategories;

namespace Crm.Api.Endpoints.Leads.GetCrmLeadCategories;

internal static class GetCrmLeadCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories", HandleAsync).WithName("getCrmLeadCategories");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetLeadCategoriesQuery(), cancellationToken)).ToHttpResult();
}
