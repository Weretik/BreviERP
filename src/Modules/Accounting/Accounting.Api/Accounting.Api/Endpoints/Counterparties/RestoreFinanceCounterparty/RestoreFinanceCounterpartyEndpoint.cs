using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;
using Accounting.Api.Endpoints.Shared;

namespace Accounting.Api.Endpoints.Counterparties.RestoreFinanceCounterparty;

internal static class RestoreFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/restore", HandleAsync).WithName("restoreFinanceCounterparty")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetFinanceCounterpartyActiveCommand(id, true, input.Version,
            AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
