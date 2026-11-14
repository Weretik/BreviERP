using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.ReviewFinanceTransaction;

namespace Accounting.Api.Endpoints.Transactions.ReviewFinanceTransaction;

internal static class ReviewFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}/review", HandleAsync).WithName("reviewFinanceTransaction");

    private static async Task<IResult> HandleAsync(Guid id, FinanceTransactionReviewInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ReviewFinanceTransactionCommand(id, input.CategoryId, input.Counterparty,
            input.OneoffName, input.Version, AccountingAccess.By(user), input.RememberCategory),
                cancellationToken)).ToHttpResult();
}
