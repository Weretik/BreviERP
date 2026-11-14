using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.UpdateFinanceAccount;

namespace Accounting.Api.Endpoints.Accounts.UpdateFinanceAccount;

internal static class UpdateFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceAccount");

    private static async Task<IResult> HandleAsync(Guid id, FinanceAccountUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceAccountCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
