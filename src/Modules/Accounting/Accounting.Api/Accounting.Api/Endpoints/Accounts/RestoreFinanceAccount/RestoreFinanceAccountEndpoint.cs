using Accounting.Application.Features.Accounts.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.RestoreFinanceAccount;

internal static class RestoreFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/restore", HandleAsync).WithName("restoreFinanceAccount")
            .WithSummary("«Відновити» an archived account")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<FinanceAccountResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetFinanceAccountActiveCommand(id, true, input.Version, AccountingAccess.By(user)),
                cancellationToken)).ToHttpResult();
}
