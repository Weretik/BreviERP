using Accounting.Application.Features.Statements.Shared.Responses;
using Accounting.Api.Access;
using Accounting.Application.Features.Statements.Commands.RollbackBankStatement;
using Accounting.Api.Endpoints.Shared;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.RollbackBankStatement;

/// <summary>«Відкотити виписку»: cancels what the statement booked and turns it into a preview again.</summary>
internal static class RollbackBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/rollback", HandleAsync).WithName("rollbackBankStatement")
            .WithSummary("Rollback bank statement")
            .WithDescription("«Відкотити виписку» (stage 7): every active operation and transfer the statement booked is cancelled (Void, kept in the journal and the change log) and gives up its bank identity, so the lines are no duplicates when booked again; a record cancelled by hand before stays as it is. Unconfirmed finance counterparties the booking created are archived when no active operation outside the statement uses them. The statement becomes a Preview again with its lines unlinked. One save.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<BankStatementResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new RollbackBankStatementCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
