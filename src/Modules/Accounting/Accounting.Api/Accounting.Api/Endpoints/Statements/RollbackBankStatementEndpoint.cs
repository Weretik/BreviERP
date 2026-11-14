using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Features.Statements.Commands.RollbackBankStatement;

namespace Accounting.Api.Endpoints.Statements;

/// <summary>«Відкотити виписку»: cancels what the statement booked and turns it into a preview again.</summary>
internal static class RollbackBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/rollback", HandleAsync).WithName("rollbackBankStatement")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new RollbackBankStatementCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
