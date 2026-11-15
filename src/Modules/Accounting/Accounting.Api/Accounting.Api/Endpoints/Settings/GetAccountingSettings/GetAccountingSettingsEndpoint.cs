using Accounting.Application.Features.Settings.Queries.GetAccountingSettings;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Settings.GetAccountingSettings;

internal static class GetAccountingSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAccountingSettings")
            .WithSummary("Cutover date between «Домашня бухгалтерія» and the current books, the time zone and today in Kyiv")
            .Produces<AccountingSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetAccountingSettingsQuery(), cancellationToken)).ToHttpResult();
}
