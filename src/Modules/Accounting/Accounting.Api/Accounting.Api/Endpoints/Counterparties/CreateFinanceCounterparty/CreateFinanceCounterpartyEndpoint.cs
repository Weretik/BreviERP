using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;

namespace Accounting.Api.Endpoints.Counterparties.CreateFinanceCounterparty;

internal static class CreateFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceCounterparty");

    private static async Task<IResult> HandleAsync(FinanceCounterpartyInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateFinanceCounterpartyCommand(input.ToFields(), input.Verified, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
