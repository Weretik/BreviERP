using Accounting.Application.Features.Accounts.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.ArchiveFinanceAccount;

internal static class ArchiveFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/archive", HandleAsync).WithName("archiveFinanceAccount")
            .WithSummary("«Архівувати»: the account leaves the active list")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<FinanceAccountResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetFinanceAccountActiveCommand(id, false, input.Version, AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
