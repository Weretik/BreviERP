using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;

namespace Accounting.Api.Endpoints.Accounts.SetFinanceAccountCutover;

internal static class SetFinanceAccountCutoverEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}/cutover", HandleAsync).WithName("setFinanceAccountCutover");

    private static async Task<IResult> HandleAsync(Guid id, FinanceAccountCutoverInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetCutoverBalanceCommand(id, input.BalanceMinor, input.Confirmed,
            input.Version, AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
