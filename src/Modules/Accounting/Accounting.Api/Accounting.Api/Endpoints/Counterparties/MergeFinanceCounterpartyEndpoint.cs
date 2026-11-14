using Accounting.Api.Counterparties;
using Accounting.Api.Access;
using Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;

namespace Accounting.Api.Endpoints.Counterparties;

/// <summary>Joins <paramref name="id"/> into the target; returns the target.</summary>
internal static class MergeFinanceCounterpartyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/merge", HandleAsync).WithName("mergeFinanceCounterparty")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceCounterpartyMergeInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new MergeFinanceCounterpartyCommand(id, input.Version, input.TargetId,
            input.TargetVersion, AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
