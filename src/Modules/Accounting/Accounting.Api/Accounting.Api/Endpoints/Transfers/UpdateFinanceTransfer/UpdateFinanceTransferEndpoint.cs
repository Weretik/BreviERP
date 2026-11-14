using Accounting.Api.Access;
using Accounting.Application.Features.Transfers.Commands.UpdateFinanceTransfer;

namespace Accounting.Api.Endpoints.Transfers.UpdateFinanceTransfer;

internal static class UpdateFinanceTransferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceTransfer");

    private static async Task<IResult> HandleAsync(Guid id, FinanceTransferUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceTransferCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
