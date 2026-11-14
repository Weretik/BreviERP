using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.PromoteFinanceOneoff;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transactions.PromoteFinanceOneoff;

internal static class PromoteFinanceOneoffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/promote-oneoff", HandleAsync).WithName("promoteFinanceOneoff")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new PromoteFinanceOneoffCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
