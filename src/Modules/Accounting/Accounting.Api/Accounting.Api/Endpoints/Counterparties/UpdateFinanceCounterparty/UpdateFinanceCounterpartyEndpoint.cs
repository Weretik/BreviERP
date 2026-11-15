using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.UpdateFinanceCounterparty;

internal static class UpdateFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceCounterparty")
            .WithSummary("Edit a finance counterparty")
            .WithDescription("Edit a finance counterparty. The default category must be of the current books; auto-apply needs a confirmed counterparty with a default category.")
            .Produces<FinanceCounterpartyResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceCounterpartyUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceCounterpartyCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
