using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Features.Statements.Commands.BookBankStatement;

namespace Accounting.Api.Endpoints.Statements;

internal static class BookBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:guid}/book", HandleAsync).WithName("bookBankStatement")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(Guid id, FinanceVersionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new BookBankStatementCommand(id, input.Version, AccountingAccess.By(user)),
            cancellationToken)).ToHttpResult();
}
