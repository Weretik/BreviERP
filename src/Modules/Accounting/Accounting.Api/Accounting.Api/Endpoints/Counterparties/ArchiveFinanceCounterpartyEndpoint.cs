using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;

namespace Accounting.Api.Endpoints.Counterparties;

internal static class ArchiveFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/archive", HandleAsync).WithName("archiveFinanceCounterparty")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetFinanceCounterpartyActiveCommand(id, false, input.Version,
            AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
