using Accounting.Application.Features.Transactions.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.VoidFinanceTransaction;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transactions.VoidFinanceTransaction;

internal static class VoidFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/void", HandleAsync).WithName("voidFinanceTransaction")
            .WithSummary("«Анулювати»: never deleted; the operation stays in the journal (includeVoided) and the log but no longer counts")
            .WithDescription("«Анулювати»: never deleted; the operation stays in the journal (includeVoided) and the log but no longer counts. A cancelled operation cannot be changed (400).")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<FinanceTransactionResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new VoidFinanceTransactionCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
