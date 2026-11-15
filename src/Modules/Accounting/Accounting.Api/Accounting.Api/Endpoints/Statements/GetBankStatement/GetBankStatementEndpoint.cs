using Accounting.Application.Features.Statements.Queries.GetBankStatement.Results;
using Accounting.Application.Features.Statements.Queries.GetBankStatement;
using Accounting.Domain.Statements;
using Accounting.Domain.Statements.Enums;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Statements.GetBankStatement;

internal static class GetBankStatementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}", HandleAsync).WithName("getBankStatement")
            .WithSummary("Get bank statement")
            .WithDescription("A statement with 100 of its lines in file order (optionally of one state), the number of lines in each state and what book would add now.")
            .Produces<BankStatementDetailsResponse>();

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        QueryEnum<BankLineState>? state = null, int page = 1, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetBankStatementQuery(id, state?.Value, page), cancellationToken)).ToHttpResult();
}
