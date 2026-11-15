using Reference.Application.Features.PricingParameter.Queries.GetPricingParameters;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.GetPricingParameters;

internal static class GetPricingParametersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync)
            .WithName("getPricingParameters")
            .WithSummary("List pricing parameters in sheet order")
            .WithDescription("Anonymous. A calculated parameter (isCalculated) has its value computed from other parameters, for example MINUTE_COST_SEAMSTRESS = SALARY_SEAMSTRESS / (WORK_DAYS_MONTH × MINUTES_SHIFT); null when an input is missing. An empty list when there are none.")
            .Produces<List<PricingParameterRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPricingParametersQuery(), cancellationToken)).ToHttpResult();
}
