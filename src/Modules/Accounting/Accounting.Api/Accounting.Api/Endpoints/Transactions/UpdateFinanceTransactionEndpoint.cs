using Accounting.Api.Transactions;
using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.UpdateFinanceTransaction;

namespace Accounting.Api.Endpoints.Transactions;

internal static class UpdateFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceTransaction");

    private static async Task<IResult> HandleAsync(Guid id, FinanceTransactionUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceTransactionCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
