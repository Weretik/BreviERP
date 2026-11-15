using Crm.Application.Features.Quotes.Shared.Settings;
using Crm.Application.Features.Quotes.Queries.GetQuoteSettings;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteSettings;

internal static class GetCrmQuoteSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmQuoteSettings")
            .WithSummary("«Налаштування КП» (R-13)")
            .WithDescription("«Налаштування КП» (R-13). Before the first save returns the defaults with version 0. Has a contact row for every active staff member; an unsaved contact shows the account name, phone and email.")
            .Produces<QuoteSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteSettingsQuery(), cancellationToken)).ToHttpResult();
}
