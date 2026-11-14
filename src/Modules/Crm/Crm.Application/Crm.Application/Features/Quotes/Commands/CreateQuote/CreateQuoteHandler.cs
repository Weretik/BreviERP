using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using BuildingBlocks.Application;

namespace Crm.Application.Features.Quotes.Commands.CreateQuote;

public sealed class CreateQuoteHandler(
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
