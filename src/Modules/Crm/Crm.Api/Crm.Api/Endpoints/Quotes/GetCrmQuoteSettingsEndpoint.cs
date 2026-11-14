using Crm.Application.Features.Quotes.Queries.GetQuoteSettings;

namespace Crm.Api.Endpoints.Quotes;

internal static class GetCrmQuoteSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmQuoteSettings");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteSettingsQuery(), cancellationToken)).ToHttpResult();
}
