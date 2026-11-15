using Accounting.Application.Features.Transactions.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Transactions.CreateFinanceTransaction;

internal static class CreateFinanceTransactionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceTransaction")
            .WithSummary("«Додати дохід» / «Додати витрату»")
            .WithDescription("«Додати дохід» / «Додати витрату». The date must fall into the area (Current from 2026-10-01) and be on or after the account opening date; Current needs an active account; the category must be active, of the same area and direction. The currency comes from the account. A finance counterparty that is not confirmed yet gives verification NeedsCounterpartyReview; an archived counterparty cannot be newly chosen.")
            .Produces<FinanceTransactionResponse>();

    private static async Task<IResult> HandleAsync(FinanceTransactionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new CreateFinanceTransactionCommand(input.Scope, input.ToFields(), AccountingAccess.By(user),
                input.RequestId),
                cancellationToken)).ToHttpResult();
}
