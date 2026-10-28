using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.RepriceQuote.Mapping;
using Crm.Domain.Quotes.Versions;
using Mediator;

namespace Crm.Application.Quotes.RepriceQuote;

public sealed class RepriceQuoteCommandHandler(IQuoteMutationStore store, QuoteVersionBuilder builder)
    : ICommandHandler<RepriceQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(RepriceQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.VersionId == Guid.Empty || command.ExpectedRevision <= 0 ||
            command.Bases is null || command.Bases.Count is < 1 or > 3000 ||
            command.Bases.Any(x => x is null || x.VariantId == Guid.Empty || x.BasisToken == Guid.Empty) ||
            command.Bases.Select(x => x.VariantId).Distinct().Count() != command.Bases.Count)
            return Result.Invalid(new ValidationError("bases", "invalid_reprice_bases"));
        return await store.ExecuteAsync("RepriceQuote", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var quote = await session.FindQuoteAsync(command.QuoteId, ct);
                if (quote is null || (!command.Actor.IsAdmin && quote.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                quote.AssertEditable(command.ExpectedRevision, command.Actor.UserId, command.Actor.IsAdmin);
                var previous = await session.FindVersionAsync(quote.CurrentVersionId!.Value, ct);
                if (previous is null || previous.PublicId != command.VersionId)
                    return Result.Conflict("version_changed");
                var tokens = command.Bases.ToDictionary(x => x.VariantId, x => x.BasisToken);
                var variants = previous.Items.SelectMany(x => x.Variants).ToArray();
                if (variants.Length != tokens.Count || variants.Any(x => !tokens.ContainsKey(x.PublicId)))
                    return Result.Invalid(new ValidationError("bases", "all_variants_required"));
                var content = QuoteRepriceSnapshot.ToContent(previous, tokens);
                var built = await builder.BuildAsync(content, previous, quote.PublicId, command.Actor, ct);
                if (!built.IsSuccess) return QuoteBuildResult.Failure(built);
                if (QuoteRepriceSnapshot.HasBlockedPrices(built.Value))
                    return Result.Conflict("reprice_blocked");
                var version = QuoteVersion.Create(quote.Id, quote.CurrentVersionNumber + 1, previous.Id,
                    "recalculated", previous.RecipientName, previous.CustomerPublicId,
                    previous.ContactPublicId, command.Actor.UserId, built.Value);
                session.Add(version);
                await session.FlushAsync(ct);
                quote.AppendVersion(command.ExpectedRevision, version.Id, command.Actor.UserId, "recalculated");
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(quote, version, true));
            }, cancellationToken);
    }
}
