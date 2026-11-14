using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.CreateQuote;

public sealed class CreateQuoteCommandHandler(
    IQuoteStaffReader staff, QuoteItemsBuilder builder, QuoteNumbering numbering, TimeProvider clock)
    : ICommandHandler<CreateQuoteCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(CreateQuoteCommand command, CancellationToken cancellationToken)
    {
        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var input = command.Input;
        var header = new QuoteHeader(input.InternalName, input.CustomerId, input.RecipientName, input.CompanyName,
            input.ClientComment, input.ManagerNote);
        var added = await numbering.AddAsync(now, async number =>
        {
            var items = await builder.BuildAsync(input.Items, [], author, now, cancellationToken);
            if (!items.IsSuccess) return items.Map(_ => default(Quote)!);
            var created = Quote.Create(number, QuoteKind.Request, author, header, items.Value, now);
            return created.IsSuccess ? Result.Success(created.Value) : created.Error!.ToResult<Quote>();
        }, cancellationToken);
        return added.Map(quote => quote.ToResponse());
    }
}
