using Accounting.Application.Features.Transactions.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.UpdateFinanceTransaction;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transactions.UpdateFinanceTransaction;

internal static class UpdateFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceTransaction")
            .WithSummary("Edit an active hand-made operation; the area stays")
            .WithDescription("Edit an active hand-made operation; the area stays. An unchanged account or category may already be archived or switched off; a new date must fit the area.")
            .Produces<FinanceTransactionResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceTransactionUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceTransactionCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
