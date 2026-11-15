using Accounting.Application.Features.Transfers.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Transfers.Commands.VoidFinanceTransfer;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transfers.VoidFinanceTransfer;

internal static class VoidFinanceTransferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/void", HandleAsync).WithName("voidFinanceTransfer")
            .WithSummary("«Анулювати» a transfer: it stays in the journal and the log but no longer moves money")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<FinanceTransferResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new VoidFinanceTransferCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
