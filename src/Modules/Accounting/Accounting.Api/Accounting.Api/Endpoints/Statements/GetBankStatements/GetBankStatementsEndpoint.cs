using Accounting.Application.Features.Statements.Shared.Responses;
using Accounting.Application.Features.Statements.Queries.GetBankStatements;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.GetBankStatements;

internal static class GetBankStatementsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getBankStatements")
            .WithSummary("«Історія виписок» (stage 6) — the last 100 uploaded statements of all accounts, newest first")
            .Produces<IReadOnlyList<BankStatementResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetBankStatementsQuery(), cancellationToken)).ToHttpResult();
}
