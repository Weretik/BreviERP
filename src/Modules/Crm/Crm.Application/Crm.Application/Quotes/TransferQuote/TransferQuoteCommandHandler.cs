using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Ownership;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Customers.Identifiers;
using Mediator;

namespace Crm.Application.Quotes.TransferQuote;

public sealed class TransferQuoteCommandHandler(IQuoteMutationStore store, IQuoteAssigneeReader assignees)
    : ICommandHandler<TransferQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(TransferQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.ExpectedRevision <= 0 || command.OwnerId <= 0 ||
            string.IsNullOrWhiteSpace(command.Reason) || command.Reason.Length > 1000)
            return Result.Invalid(new ValidationError("transfer", "invalid_transfer"));
        return await store.ExecuteAsync("TransferQuote", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var quote = await session.FindQuoteAsync(command.QuoteId, ct);
                if (quote is null || (!command.Actor.IsAdmin && quote.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                quote.AssertEditable(command.ExpectedRevision, command.Actor.UserId, command.Actor.IsAdmin);
                if (!await assignees.IsEligibleAsync(command.OwnerId, ct))
                    return Result.Invalid(new ValidationError("ownerId", "ineligible_assignee"));
                var version = await session.FindVersionAsync(quote.CurrentVersionId!.Value, ct);
                if (version is null) return Result.Conflict("missing_current_version");
                quote.Transfer(command.ExpectedRevision, new CrmUserId(command.OwnerId),
                    command.Actor.UserId, command.Actor.IsAdmin, command.Reason);
                await session.FlushAsync(ct);
                // Return only the snapshot the initiating actor was authorized to transfer.
                return Result.Success(QuoteDetailMapper.Map(quote, version,
                    command.Actor.IsAdmin || command.OwnerId == command.Actor.UserId));
            }, cancellationToken);
    }
}
