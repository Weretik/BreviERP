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
            var created = Quote.Create(number, QuoteKind.Introduction, author, input.ToHeader(), items.Value, now);
            if (created.IsFailure) return created.Error!.ToResult<Quote>();
            var introduced = created.Value.SetIntroduction(introduction);
            return introduced.IsSuccess ? Result.Success(created.Value) : introduced.Error!.ToResult<Quote>();
        }, cancellationToken);
        return added.Map(quote => quote.ToResponse());
    }
}
