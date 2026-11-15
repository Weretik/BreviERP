using Accounting.Application.Features.Accounts.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.CreateFinanceAccount;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.CreateFinanceAccount;

internal static class CreateFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceAccount")
            .WithSummary("«Додати рахунок»")
            .WithDescription("«Додати рахунок». The name is trimmed, the currency upper-cased, the IBAN keeps letters and digits.")
            .Produces<FinanceAccountResponse>();

    private static async Task<IResult> HandleAsync(FinanceAccountInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFinanceAccountCommand(input.ToFields(), AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
