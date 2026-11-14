using Crm.Application.Features.Quotes.Queries.GetQuoteIntroductionSetup;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>What «КП для знайомства» starts from: the main model of each season and the letter variants.</summary>
internal static class GetCrmQuoteIntroductionSetupEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("introduction", HandleAsync).WithName("getCrmQuoteIntroductionSetup");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteIntroductionSetupQuery(), cancellationToken)).ToHttpResult();
}
