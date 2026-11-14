using Accounting.Api.Access;
using Accounting.Application.Features.Transfers.Commands.CreateFinanceTransfer;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transfers.CreateFinanceTransfer;

internal static class CreateFinanceTransferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceTransfer");

    private static async Task<IResult> HandleAsync(FinanceTransferInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateFinanceTransferCommand(input.Scope, input.ToFields(), AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
