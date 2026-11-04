using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Introductions.CreateIntroduction;

public sealed class CreateQuoteIntroductionCommandHandler(
    IQuoteStaffReader staff, QuoteIntroductionBuilder builder, QuoteNumbering numbering, TimeProvider clock)
    : ICommandHandler<CreateQuoteIntroductionCommand, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(CreateQuoteIntroductionCommand command,
        CancellationToken cancellationToken)
    {
        var author = await staff.PersonAsync(command.Actor, cancellationToken);
        if (author is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var now = clock.GetUtcNow();
        var input = command.Input;
        var introduction = input.ToIntroduction();
        var added = await numbering.AddAsync(now, async number =>
        {
            var items = await builder.BuildAsync(introduction, input.Pages, [], author, now, cancellationToken);
            if (!items.IsSuccess) return items.Map(_ => default(Quote)!);
            var quote = new Quote(number, QuoteKind.Introduction, author, input.ToHeader(), items.Value, now);
            quote.SetIntroduction(introduction);
            return Result.Success(quote);
        }, cancellationToken);
        return added.Map(quote => quote.ToResponse());
    }
}
