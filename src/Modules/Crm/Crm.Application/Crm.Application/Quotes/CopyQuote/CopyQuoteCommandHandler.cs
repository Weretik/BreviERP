using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;
using Mediator;

namespace Crm.Application.Quotes.CopyQuote;

public sealed class CopyQuoteCommandHandler(IQuoteMutationStore store)
    : ICommandHandler<CopyQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(CopyQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.VersionId == Guid.Empty || command.ExpectedRevision <= 0)
            return Result.Invalid(new ValidationError("copy", "invalid_copy"));
        return await store.ExecuteAsync("CopyQuote", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var source = await session.FindQuoteAsync(command.QuoteId, ct);
                if (source is null || (!command.Actor.IsAdmin && source.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                if (source.Revision != command.ExpectedRevision) return Result.Conflict("revision_conflict");
                var version = await session.FindVersionAsync(source.Id, command.VersionId, ct);
                if (version is null) return Result.NotFound();
                if (version.IntroductionPublicSnapshot is not null)
                    return Result.Conflict("introduction_requires_new_from_template");
                var copy = Quote.Create(source.CustomerId, version.ContactPublicId,
                    version.RecipientName, new CrmUserId(command.Actor.UserId));
                copy.SetCopySource(source.PublicId, version.PublicId);
                session.Add(copy);
                await session.FlushAsync(ct);
                var copied = QuoteVersion.Create(copy.Id, 1, null, "copied", version.RecipientName,
                    version.CustomerPublicId, version.ContactPublicId, command.Actor.UserId,
                    QuoteSnapshotCloner.Clone(version));
                session.Add(copied);
                await session.FlushAsync(ct);
                copy.SetInitialVersion(copied.Id);
                copy.Record("copied", command.Actor.UserId);
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(copy, copied, true));
            }, cancellationToken);
    }
}
