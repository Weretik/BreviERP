using Ardalis.Result;
using Crm.Application.CustomSewing.Persistence;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Drafts;
using Mediator;
namespace Crm.Application.CustomSewing.ApplyTemplate;

public sealed class ApplyTemplateCommandHandler(ICustomMutationStore mutations, ITemplateReadStore templates, ITemplateApplySource source)
    : ICommandHandler<ApplyTemplateCommand, Result<AppliedTemplate>>
{
    public async ValueTask<Result<AppliedTemplate>> Handle(ApplyTemplateCommand command, CancellationToken ct)
    {
        if (command.TemplateId == Guid.Empty || command.VersionId == Guid.Empty || command.RequestId == Guid.Empty
            || command.ExpectedRevision <= 0 || command.Actor.UserId <= 0)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        // Scope is checked before replay as well, without revealing a previous response to a newly unauthorized actor.
        var selected = await templates.DetailAsync(command.TemplateId, null, command.Actor, ct);
        if (selected is null) return Result.NotFound();
        var result = await mutations.ExecuteAsync("ApplyCustomTemplate", command.RequestId, command.Actor, command, async (session, token) =>
        {
            var current = await source.LockAsync(command.TemplateId, command.VersionId, command.Actor, token);
            if (current is null) return Result.NotFound();
            if (current.Revision != command.ExpectedRevision) return Result.Conflict("revision_conflict");
            if (current.Archived) return Result.Conflict("template_archived");
            var customer = command.CustomerPublicId is { } customerId
                ? await session.FindCustomerAsync(customerId, command.Actor, token) : null;
            if (command.CustomerPublicId is not null && customer is null) return Result.NotFound();
            if (customer is { Active: false } || customer?.MergedIntoId is not null)
                return Result.Invalid(new ValidationError("customerId", "inactive_customer"));
            var quote = command.QuotePublicId is { } quoteId
                ? await session.FindQuoteAsync(quoteId, command.Actor, token) : null;
            if (command.QuotePublicId is not null && quote is null) return Result.NotFound();
            if (quote?.Archived == true) return Result.Conflict("quote_not_editable");
            if (customer is not null && quote?.CustomerId is { } linked && linked != customer.Id)
                return Result.Invalid(new ValidationError("customerId", "quote_customer_mismatch"));
            var draft = CustomSewingDraft.Create(new CrmUserId(command.Actor.UserId), customer?.Id ?? quote?.CustomerId,
                quote?.Id, current.Selections.ToDraft());
            draft.SetTemplateOrigin(new CustomTemplateOrigin(current.PublicId, current.VersionId, current.Introduction));
            session.Add(draft);
            await session.FlushAsync(token);
            return Result.Success(await session.DetailAsync(draft, token));
        }, ct);
        return result.IsSuccess ? Result.Success(new AppliedTemplate(result.Value, result.Value.TemplateOrigin!.VersionPublicId, result.Value.TemplateOrigin.Introduction))
            : result.Status switch
            {
                ResultStatus.NotFound => Result.NotFound(),
                ResultStatus.Conflict => Result.Conflict(result.Errors.ToArray()),
                ResultStatus.Invalid => Result.Invalid(result.ValidationErrors),
                _ => Result.Error(string.Join(", ", result.Errors))
            };
    }
}
