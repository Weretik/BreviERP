using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes.Versions;
using Mediator;

namespace Crm.Application.Quotes.SaveQuoteVersion;

public sealed class SaveQuoteVersionCommandHandler(
    IQuoteMutationStore store, QuoteVersionBuilder builder)
    : ICommandHandler<SaveQuoteVersionCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(
        SaveQuoteVersionCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.RequestId == Guid.Empty ||
            command.ExpectedRevision <= 0 || command.Content is null || command.Actor.UserId <= 0)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        return await store.ExecuteAsync("SaveQuoteVersion", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var quote = await session.FindQuoteAsync(command.QuoteId, ct);
                if (quote is null || (!command.Actor.IsAdmin && quote.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                quote.AssertEditable(command.ExpectedRevision, command.Actor.UserId, command.Actor.IsAdmin);
                var previous = quote.CurrentVersionId is { } id
                    ? await session.FindVersionAsync(id, ct)
                    : null;
                if (previous is null)
                    return Result.Conflict("missing_current_version");
                var built = await builder.BuildAsync(command.Content, previous, quote.PublicId, command.Actor, ct);
                if (!built.IsSuccess) return QuoteBuildResult.Failure(built);
                var version = QuoteVersion.Create(quote.Id, quote.CurrentVersionNumber + 1,
                    previous.Id, "edited", previous.RecipientName, previous.CustomerPublicId,
                    previous.ContactPublicId, command.Actor.UserId, built.Value);
                session.Add(version);
                await session.FlushAsync(ct);
                quote.AppendVersion(command.ExpectedRevision, version.Id);
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(quote, version, true));
            }, cancellationToken);
    }
}
