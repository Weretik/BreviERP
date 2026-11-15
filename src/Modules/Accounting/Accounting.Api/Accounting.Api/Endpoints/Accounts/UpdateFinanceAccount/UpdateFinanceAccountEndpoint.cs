using Accounting.Application.Features.Accounts.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.UpdateFinanceAccount;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.UpdateFinanceAccount;

internal static class UpdateFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceAccount")
            .WithSummary("Edit an account")
            .WithDescription("Edit an account. A save that changes nothing keeps the version. With movements the currency is fixed and the opening date cannot be later than the earliest movement (400).")
            .Produces<FinanceAccountResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceAccountUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new UpdateFinanceAccountCommand(id, input.ToFields(), input.Version, AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
