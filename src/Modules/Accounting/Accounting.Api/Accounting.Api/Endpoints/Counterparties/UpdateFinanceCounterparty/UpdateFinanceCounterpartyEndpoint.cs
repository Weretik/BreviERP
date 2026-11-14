using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.UpdateFinanceCounterparty;

internal static class UpdateFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceCounterparty");

    private static async Task<IResult> HandleAsync(Guid id, FinanceCounterpartyUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceCounterpartyCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
