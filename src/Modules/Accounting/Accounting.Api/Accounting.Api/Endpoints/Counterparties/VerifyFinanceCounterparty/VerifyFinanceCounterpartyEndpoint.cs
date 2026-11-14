using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Counterparties.VerifyFinanceCounterparty;

internal static class VerifyFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/verify", HandleAsync).WithName("verifyFinanceCounterparty")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new VerifyFinanceCounterpartyCommand(id, input.Version,
            AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
