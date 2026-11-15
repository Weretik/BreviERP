using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
using Catalog.Application.Features.Pricing.Queries.GetPpeRule;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpePricingRule;

internal static class GetPpePricingRuleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpePricingRule")
            .WithSummary("Get PPE pricing rule")
            .WithDescription("CatalogRead. Latest exact-code rule, including an imported draft.")
            .RequireAuthorization("CatalogRead")
            .Produces<PpeRuleVersion>();

    private static async Task<IResult> HandleAsync(string code, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeRuleQuery(code), cancellationToken)).ToHttpResult();
}
