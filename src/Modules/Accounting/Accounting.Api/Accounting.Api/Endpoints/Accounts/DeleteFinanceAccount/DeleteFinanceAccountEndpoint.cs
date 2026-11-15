using Accounting.Api.Access;
using Accounting.Application.Features.Accounts.Commands.DeleteFinanceAccount;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Accounts.DeleteFinanceAccount;

internal static class DeleteFinanceAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:guid}", HandleAsync).WithName("deleteFinanceAccount")
            .WithSummary("Delete an account; the change log keeps its last snapshot")
            .WithDescription("Delete an account; the change log keeps its last snapshot. An account with any operation or transfer (cancelled ones too) can only be archived: 409.");

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default) =>
        (await sender.Send(new DeleteFinanceAccountCommand(id, version, AccountingAccess.By(user)),
            cancellationToken)).ToNoContentHttpResult();
}
