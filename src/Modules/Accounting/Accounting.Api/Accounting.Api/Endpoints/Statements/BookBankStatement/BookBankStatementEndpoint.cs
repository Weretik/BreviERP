using Accounting.Application.Features.Statements.Shared.Responses;
using Accounting.Api.Access;
using Accounting.Application.Features.Statements.Commands.BookBankStatement;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.BookBankStatement;

internal static class BookBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/book", HandleAsync).WithName("bookBankStatement")
            .WithSummary("Book bank statement")
            .WithDescription("«Провести виписку»: every included line of states Ready, NeedsCategory and NewCounterparty becomes an operation (source BankImport) and every Transfer line a transfer, in one save, after the exact duplicates are checked again. A line nobody was recognised for creates an unconfirmed finance counterparty (one per ЄДРПОУ, IBAN or name). The statement becomes Booked and no longer changes.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<BankStatementResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new BookBankStatementCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
