using Ardalis.Result;
using Crm.Application.Abstractions.Documents;
using Crm.Application.Documents.Numbering;
using Crm.Application.Documents.Projection;
using Crm.Application.Files.Validation;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes.Issues;
using Mediator;

namespace Crm.Application.Quotes.PrepareQuote;

public sealed class PrepareQuoteCommandHandler(IQuoteMutationStore store, IDocumentNumberAllocator numbers,
    IQuoteBrandAssetResolver brandAssets)
    : ICommandHandler<PrepareQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(PrepareQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.QuoteId == Guid.Empty || command.VersionId == Guid.Empty ||
            command.ExpectedRevision <= 0 || command.RequestId == Guid.Empty || command.Actor.UserId <= 0)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));

        return await store.ExecuteAsync("PrepareQuote", command.RequestId, command.Actor, command,
            async (session, ct) =>
            {
                var quote = await session.FindQuoteAsync(command.QuoteId, ct);
                if (quote is null || (!command.Actor.IsAdmin && quote.OwnerId.Value != command.Actor.UserId))
                    return Result.NotFound();
                quote.AssertEditable(command.ExpectedRevision, command.Actor.UserId, command.Actor.IsAdmin);
                if (quote.CurrentVersionId is not { } versionId)
                    return Result.Conflict("missing_current_version");
                var version = await session.FindVersionAsync(versionId, ct);
                if (version is null || version.PublicId != command.VersionId)
                    return Result.Conflict("version_conflict");
                var errors = QuoteReadyValidator.Validate(version);
                if (errors.Count > 0) return Result.Invalid(errors);
                var settings = await session.FindActiveDocumentVersionsAsync(ct);
                if (settings is null)
                    return Result.Invalid(new ValidationError("issuerSettings", "active_issuer_settings_required"));
                PublicDocumentImage? logo = null;
                if (settings.Issuer.LogoAssetKey is { } logoKey)
                {
                    logo = await brandAssets.ResolveAsync(logoKey, ct);
                    if (logo is null || logo.ContentType is not ("image/png" or "image/jpeg") ||
                        !PrivateFilePolicy.MatchesAllowedContent(logo.Bytes.Span, logo.ContentType))
                        return Result.Invalid(new ValidationError("logoAssetKey", "logo_asset_unavailable"));
                }

                // Rendering must be possible before the number and ready transition are committed.
                // The source worker uses this same builder and the same verified issuer contact.
                try
                {
                    QuoteDocumentProjectionSelector.Build(version, settings.Issuer,
                        settings.Template, quote.Number ?? "10/10/2026", settings.Issuer.LegalName, logo);
                }
                catch (InvalidOperationException exception) when (exception.Message is
                    "public_document_input_invalid" or "public_logo_asset_missing_or_unexpected" or
                    "public_items_missing" or "public_variants_missing" or "public_tier_price_missing" or
                    "public_tiers_missing" or "public_document_too_large" or
                    "introduction_public_snapshot_invalid" or "introduction_public_header_invalid")
                {
                    return Result.Invalid(new ValidationError("document", exception.Message));
                }

                var readyAt = DateTimeOffset.UtcNow;
                var number = quote.Number ?? await numbers.AllocateAsync(readyAt, ct);
                quote.PrepareReady(command.ExpectedRevision, number, command.Actor.UserId, command.Actor.IsAdmin);
                session.Add(QuoteIssue.Create(quote.Id, version.Id, settings.Issuer.Id,
                    settings.Template.Id, number, readyAt));
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(quote, version, true));
            }, cancellationToken);
    }
}
