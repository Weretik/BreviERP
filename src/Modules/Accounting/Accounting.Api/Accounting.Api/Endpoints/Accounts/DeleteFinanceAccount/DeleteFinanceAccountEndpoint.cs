using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.DeleteFinanceAccount;

namespace Accounting.Api.Endpoints.Accounts.DeleteFinanceAccount;

internal static class DeleteFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:guid}", HandleAsync).WithName("deleteFinanceAccount");

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default) =>
        (await sender.Send(new DeleteFinanceAccountCommand(id, version, AccountingAccess.By(user)),
            cancellationToken)).ToNoContentHttpResult();
}
