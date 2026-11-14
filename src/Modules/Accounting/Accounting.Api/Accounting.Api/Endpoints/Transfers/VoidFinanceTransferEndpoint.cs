using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Features.Transfers.Commands.VoidFinanceTransfer;

namespace Accounting.Api.Endpoints.Transfers;

internal static class VoidFinanceTransferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/void", HandleAsync).WithName("voidFinanceTransfer")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new VoidFinanceTransferCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
