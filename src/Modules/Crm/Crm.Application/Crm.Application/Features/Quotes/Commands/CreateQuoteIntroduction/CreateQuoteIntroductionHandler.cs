using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;

namespace Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;

public sealed class CreateQuoteIntroductionHandler(
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
