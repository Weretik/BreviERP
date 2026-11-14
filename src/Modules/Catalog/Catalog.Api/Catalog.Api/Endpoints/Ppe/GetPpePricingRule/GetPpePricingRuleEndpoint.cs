using Catalog.Application.Features.Pricing.Queries.GetPpeRule;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpePricingRule;

internal static class GetPpePricingRuleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpePricingRule")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(string code, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeRuleQuery(code), cancellationToken)).ToHttpResult();
}
