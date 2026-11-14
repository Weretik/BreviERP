using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Counterparties.GetFinanceCounterparties;

internal static class GetFinanceCounterpartiesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceCounterparties");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        QueryEnum<FinanceCounterpartyTab>? tab = null, string? search = null, bool includeArchived = false,
        int page = 1, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceCounterpartiesQuery(tab?.Value ?? FinanceCounterpartyTab.Financial, search,
            includeArchived, page),
            cancellationToken)).ToHttpResult();
}
