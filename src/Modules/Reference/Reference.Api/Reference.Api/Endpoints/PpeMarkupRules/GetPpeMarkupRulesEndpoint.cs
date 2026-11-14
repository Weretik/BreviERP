using Reference.Application.Features.PpeMarkupRule.Queries.GetPpeMarkupRules;

namespace Reference.Api.Endpoints.PpeMarkupRules;

internal static class GetPpeMarkupRulesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<PpeMarkupRuleRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeMarkupRulesQuery(), cancellationToken)).ToHttpResult();
}
