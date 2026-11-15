using Accounting.Application.Features.Statements.Shared.Responses;
using Accounting.Application.Features.Statements.Commands.UpdateBankLine;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Statements.UpdateBankStatementLine;

internal static class UpdateBankStatementLineEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}/lines/{lineId:guid}", HandleAsync).WithName("updateBankStatementLine")
            .WithSummary("Update bank statement line")
            .WithDescription("The director's choice for one line of a statement that is not booked — category of the current books in the direction of the line (or none), a counterparty or a one-off name (not both), description, note and excluded. A transfer line takes only the note and excluded; an error line and an exact duplicate cannot be included. The bank data never change.")
            .Produces<BankStatementLineResponse>();

    private static async Task<IResult> HandleAsync(Guid id, Guid lineId, BankStatementLineInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateBankLineCommand(id, lineId, input.CategoryId, input.Counterparty,
            input.OneoffName, input.Description, input.Comment, input.Excluded), cancellationToken)).ToHttpResult();
}
