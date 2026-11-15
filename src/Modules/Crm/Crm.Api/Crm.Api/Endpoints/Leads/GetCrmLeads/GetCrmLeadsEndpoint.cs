using Crm.Application.Features.Leads.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Leads.Queries.GetLeads;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.GetCrmLeads;

/// <param name="scope">`All` (default), `Mine` or `Free`.</param>
/// <param name="stage">`New`, `InWork`, `Talking`, `Offer`, `Client`, `Closed`, or `Open` for every stage in work.</param>
internal static class GetCrmLeadsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmLeads")
            .WithSummary("Get CRM leads")
            .WithDescription("Requires access:leads (view). A page of «Усі ліди»: leads still in the sheet first, newest added first. Search matches the number, company, region, industry, contacts and owner. Chip counts follow search, category and stage but not the scope.")
            .Produces<LeadPageResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, string? scope = null, string? category = null, string? stage = null, int page = 1,
        int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = LeadActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new GetLeadsQuery(actor, search, scope, category, stage, page, pageSize),
                cancellationToken)).ToHttpResult();
    }
}
