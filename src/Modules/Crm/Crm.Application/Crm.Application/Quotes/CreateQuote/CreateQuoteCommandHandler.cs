using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;
using Mediator;

namespace Crm.Application.Quotes.CreateQuote;

public sealed class CreateQuoteCommandHandler(
    IQuoteMutationStore store, QuoteVersionBuilder builder)
    : ICommandHandler<CreateQuoteCommand, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(
        CreateQuoteCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Actor.UserId <= 0 || command.Content is null)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        return await store.ExecuteAsync("CreateQuote", command.RequestId, command.Actor,
            (object?)command.IntroductionApplicationIntent ?? command,
            async (session, ct) =>
            {
                var customer = command.CustomerPublicId is { } customerId
                    ? await session.FindCustomerAsync(customerId, ct)
                    : null;
                if (command.CustomerPublicId is not null &&
                    (customer is null || !command.Actor.CanAccess(customer)))
                    return Result.NotFound();
                if (customer is { Active: false } || customer?.MergedIntoId is not null)
                    return Result.Invalid(new ValidationError("customerId", "inactive_customer"));
                if (command.ContactPublicId is { } contactId &&
                    (customer is null || !customer.Contacts.Any(x => x.PublicId == contactId && x.Active)))
                    return Result.Invalid(new ValidationError("contactId", "invalid_contact"));
                var recipient = customer?.DisplayName ?? command.RecipientName?.Trim();
                if (string.IsNullOrWhiteSpace(recipient))
                    return Result.Invalid(new ValidationError("recipientName", "required"));
                var quote = Quote.Create(customer?.Id, command.ContactPublicId, recipient,
                    new CrmUserId(command.Actor.UserId));
                session.Add(quote);
                await session.FlushAsync(ct);
                var built = await builder.BuildAsync(command.Content, null, null, command.Actor, ct);
                if (!built.IsSuccess) return QuoteBuildResult.Failure(built);
                if (command.IntroductionPublicSnapshot is { } pin &&
                    !IntroductionQuotePinValidator.Matches(pin, built.Value))
                    return Result.Conflict("introduction_price_or_selection_changed");
                var version = QuoteVersion.Create(quote.Id, 1, null, "created", recipient,
                    customer?.PublicId, command.ContactPublicId, command.Actor.UserId, built.Value,
                    command.IntroductionPublicSnapshot);
                session.Add(version);
                await session.FlushAsync(ct);
                quote.SetInitialVersion(version.Id);
                quote.Record("created", command.Actor.UserId);
                await session.FlushAsync(ct);
                return Result.Success(QuoteDetailMapper.Map(quote, version, true));
            }, cancellationToken);
    }
}
