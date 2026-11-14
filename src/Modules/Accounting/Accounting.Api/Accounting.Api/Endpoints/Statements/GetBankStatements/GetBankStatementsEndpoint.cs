using Accounting.Application.Features.Statements.Queries.GetBankStatements;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.GetBankStatements;

internal static class GetBankStatementsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getBankStatements");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetBankStatementsQuery(), cancellationToken)).ToHttpResult();
}
