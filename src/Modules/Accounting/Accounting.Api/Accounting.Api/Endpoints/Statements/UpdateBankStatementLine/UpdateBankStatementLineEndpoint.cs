using Accounting.Application.Features.Statements.Commands.UpdateBankLine;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.UpdateBankStatementLine;

internal static class UpdateBankStatementLineEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}/lines/{lineId:guid}", HandleAsync).WithName("updateBankStatementLine");

    private static async Task<IResult> HandleAsync(Guid id, Guid lineId, BankStatementLineInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateBankLineCommand(id, lineId, input.CategoryId, input.Counterparty,
            input.OneoffName, input.Description, input.Comment, input.Excluded), cancellationToken)).ToHttpResult();
}
