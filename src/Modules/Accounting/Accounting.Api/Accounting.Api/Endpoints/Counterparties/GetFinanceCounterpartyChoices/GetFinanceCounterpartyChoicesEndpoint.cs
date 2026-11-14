using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.GetFinanceCounterpartyChoices;

internal static class GetFinanceCounterpartyChoicesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("choices", HandleAsync).WithName("getFinanceCounterpartyChoices");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? search = null,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceCounterpartyChoicesQuery(search), cancellationToken)).ToHttpResult();
}
