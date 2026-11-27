using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.ListPpePriceSources;

internal static class ListPpePriceSourcesEndpoint
{
    /// <summary>Whoever may change a source (ACC-002 R-03); the director always may.</summary>
    private const string EditPolicy = "access:price-import:edit";

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("listPpePriceSources")
            .WithSummary("List PPE price sources")
            .WithDescription("CatalogRead. Sources of «Довідники» → «Імпорт цін» with logic, schedule, discount, product counts and the last run.")
            .RequireAuthorization("CatalogRead")
            .Produces<PpePriceSourcesResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] IAuthorizationService authorization,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPpePriceSourcesQuery(), cancellationToken);
        if (!result.IsSuccess) return result.ToHttpResult();
        var canEdit = (await authorization.AuthorizeAsync(user, EditPolicy)).Succeeded;
        return Results.Ok(new PpePriceSourcesResponse(canEdit, result.Value));
    }
}
