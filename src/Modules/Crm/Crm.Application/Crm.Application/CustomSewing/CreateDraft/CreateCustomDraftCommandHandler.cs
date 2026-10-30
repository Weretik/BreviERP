using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Mediator;
using Crm.Application.CustomSewing.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.CustomSewing.CreateDraft;

public sealed class CreateCustomDraftCommandHandler(ICustomMutationStore store)
    : ICommandHandler<CreateCustomDraftCommand, Result<CustomDetail>>
{
    public async ValueTask<Result<CustomDetail>> Handle(CreateCustomDraftCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Actor.UserId <= 0)
            return Result.Invalid(new ValidationError("request", "invalid_request"));
        return await store.ExecuteAsync("CreateCustomDraft", command.RequestId, command.Actor, command, async (session, ct) =>
        {
            if (command.Configuration is null) return Result.Invalid(new ValidationError("configuration", "required"));
            var customer = command.CustomerPublicId is { } customerId
                ? await session.FindCustomerAsync(customerId, command.Actor, ct) : null;
            if (command.CustomerPublicId is not null && customer is null) return Result.NotFound();
            if (customer is { Active: false } || customer?.MergedIntoId is not null)
                return Result.Invalid(new ValidationError("customerId", "inactive_customer"));
            var quote = command.QuotePublicId is { } quoteId
                ? await session.FindQuoteAsync(quoteId, command.Actor, ct) : null;
            if (command.QuotePublicId is not null && quote is null) return Result.NotFound();
            if (quote?.Archived == true) return Result.Conflict("quote_not_editable");
            if (customer is not null && quote?.CustomerId is { } linked && linked != customer.Id)
                return Result.Invalid(new ValidationError("customerId", "quote_customer_mismatch"));
            var draft = CustomSewingDraft.Create(new CrmUserId(command.Actor.UserId), customer?.Id ?? quote?.CustomerId,
                quote?.Id, command.Configuration);
            session.Add(draft);
            await session.FlushAsync(ct);
            return Result.Success(await session.DetailAsync(draft, ct));
        }, cancellationToken);
    }
}
