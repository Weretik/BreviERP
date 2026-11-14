using Accounting.Application.Features.Settings.Queries.GetAccountingSettings;

namespace Accounting.Api.Endpoints.Settings.GetAccountingSettings;

internal static class GetAccountingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAccountingSettings");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetAccountingSettingsQuery(), cancellationToken)).ToHttpResult();
}
