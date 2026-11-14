using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transactions.CreateFinanceTransaction;

internal static class CreateFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceTransaction");

    private static async Task<IResult> HandleAsync(FinanceTransactionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateFinanceTransactionCommand(input.Scope, input.ToFields(), AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
