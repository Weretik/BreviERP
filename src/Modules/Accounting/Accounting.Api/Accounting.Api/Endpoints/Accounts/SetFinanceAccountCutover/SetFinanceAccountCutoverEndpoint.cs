using Accounting.Application.Features.Accounts.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.SetFinanceAccountCutover;

internal static class SetFinanceAccountCutoverEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}/cutover", HandleAsync).WithName("setFinanceAccountCutover")
            .WithSummary("Set finance account cutover")
            .WithDescription("«Залишок на 01.10»: confirm the starting balance of the current books (who and when are stored) or clear it with confirmed false.")
            .Produces<FinanceAccountResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceAccountCutoverInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetCutoverBalanceCommand(id, input.BalanceMinor, input.Confirmed,
            input.Version, AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
