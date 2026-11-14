using Reference.Application.Features.PricingParameter.Queries.GetPricingParameters;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.GetPricingParameters;

internal static class GetPricingParametersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<PricingParameterRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPricingParametersQuery(), cancellationToken)).ToHttpResult();
}
