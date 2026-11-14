using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;

namespace Accounting.Api.Endpoints.Accounts.CreateFinanceAccount;

internal static class CreateFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceAccount");

    private static async Task<IResult> HandleAsync(FinanceAccountInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFinanceAccountCommand(input.ToFields(), AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
