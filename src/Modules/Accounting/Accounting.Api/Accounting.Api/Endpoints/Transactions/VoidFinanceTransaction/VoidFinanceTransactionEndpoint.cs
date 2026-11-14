using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;
using Accounting.Api.Endpoints.Shared;

namespace Accounting.Api.Endpoints.Transactions.VoidFinanceTransaction;

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
