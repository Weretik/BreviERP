using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Projection;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.Pricing;
using Mediator;

namespace Crm.Application.Quotes.Introductions;

/// <summary>Applies one owned selection through the existing quote price-basis and version pipeline.</summary>
public sealed class IntroductionQuoteCreator(
    IntroductionTemplateService templates, IQuotePriceBasisStore bases, ISender sender,
    IIntroductionQuoteReplayReader replays)
{
    public async Task<Result<QuoteDetail>> CreateAsync(CrmActor actor, Guid requestId,
        Guid templatePublicId, Guid? versionPublicId, Guid? customerPublicId,
        Guid? contactPublicId, string? recipientName, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || requestId == Guid.Empty || templatePublicId == Guid.Empty)
            return Result<QuoteDetail>.Invalid(new ValidationError("requestId", "invalid_request"));
        var application = new IntroductionQuoteApplicationIntent(templatePublicId, versionPublicId,
            customerPublicId, contactPublicId, recipientName);
        var replay = await replays.ReadAsync(requestId, actor, application, cancellationToken);
        if (replay is not null) return replay;
        var applied = await templates.ApplyAsync(actor, templatePublicId, versionPublicId, cancellationToken);
        if (!applied.IsSuccess) return PreserveFailure(applied);
        var draft = applied.Value;
        var priceBases = new IntroductionPriceBasisIssuer(bases);
        var previews = new List<QuotePricePreview>(draft.Slots.Count);
        foreach (var slot in draft.Slots)
        {
            var issued = await priceBases.IssueAsync(
                requestId, actor, slot, cancellationToken);
            if (!issued.IsSuccess) return PreserveFailure(issued);
            previews.Add(issued.Value);
        }
        try
        {
            var content = IntroductionQuoteContentBuilder.Build(draft, previews);
            var pin = IntroductionPublicDocumentProjectionBuilder.Freeze(draft);
            return await sender.Send(new CreateQuoteCommand(customerPublicId, contactPublicId,
                recipientName, content, requestId, actor, pin, application), cancellationToken);
        }
        catch (InvalidOperationException exception) when (exception.Message is
            "introduction_slots_invalid" or "introduction_price_or_selection_changed" or
            "introduction_public_snapshot_invalid")
        {
            return Result<QuoteDetail>.Conflict(exception.Message);
        }
    }

    private static Result<QuoteDetail> PreserveFailure<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.Invalid => Result<QuoteDetail>.Invalid(result.ValidationErrors),
        ResultStatus.Conflict => Result<QuoteDetail>.Conflict(result.Errors.ToArray()),
        ResultStatus.NotFound => Result<QuoteDetail>.NotFound(),
        ResultStatus.Forbidden => Result<QuoteDetail>.Forbidden(),
        _ => Result<QuoteDetail>.Error(string.Join(",", result.Errors))
    };
}
