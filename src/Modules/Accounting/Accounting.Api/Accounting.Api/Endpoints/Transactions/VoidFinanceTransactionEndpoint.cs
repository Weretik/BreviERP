using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;

namespace Accounting.Api.Endpoints.Transactions;

internal static class VoidFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/void", HandleAsync).WithName("voidFinanceTransaction")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new VoidFinanceTransactionCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
