using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.ArchiveQuote;

public sealed class ArchiveQuoteCommandHandler(IQuoteMutationStore store)
    : ICommandHandler<ArchiveQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(ArchiveQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.ExpectedRevision <= 0 ||
            string.IsNullOrWhiteSpace(command.Reason) || command.Reason.Length > 1000)
            return Result.Invalid(new ValidationError("archive", "invalid_archive"));
        return await store.ExecuteAsync("ArchiveQuote", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var quote = await session.FindQuoteAsync(command.QuoteId, ct);
                if (quote is null || (!command.Actor.IsAdmin && quote.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                var version = quote.CurrentVersionId is { } id ? await session.FindVersionAsync(id, ct) : null;
                if (version is null) return Result.Conflict("missing_current_version");
                quote.SetArchived(command.ExpectedRevision, command.Archived,
                    command.Actor.UserId, command.Actor.IsAdmin, command.Reason);
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(quote, version, true));
            }, cancellationToken);
    }
}
