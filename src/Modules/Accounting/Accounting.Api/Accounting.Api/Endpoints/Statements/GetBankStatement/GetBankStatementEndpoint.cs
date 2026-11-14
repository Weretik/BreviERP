using Accounting.Application.Features.Statements.Queries.GetBankStatement;
using Accounting.Domain.Statements;

namespace Accounting.Api.Endpoints.Statements.GetBankStatement;

internal static class GetBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}", HandleAsync).WithName("getBankStatement");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        QueryEnum<BankLineState>? state = null, int page = 1, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetBankStatementQuery(id, state?.Value, page), cancellationToken)).ToHttpResult();
}
