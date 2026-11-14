using Accounting.Api.Accounts;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;

namespace Accounting.Api.Endpoints.Accounts;

internal static class RestoreFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/restore", HandleAsync).WithName("restoreFinanceAccount")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetFinanceAccountActiveCommand(id, true, input.Version, AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
