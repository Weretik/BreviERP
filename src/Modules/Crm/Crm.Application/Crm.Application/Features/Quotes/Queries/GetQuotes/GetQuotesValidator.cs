using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Enums;

namespace Crm.Application.Features.Quotes.Queries.GetQuotes;

public sealed class GetQuotesValidator : AbstractValidator<GetQuotesQuery>
{
    public const int MaxPageSize = 100;

    public GetQuotesValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Search).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Status).Must(x => Enum.TryParse<QuoteStatus>(x, false, out var status) && Enum.IsDefined(status))
            .When(x => !string.IsNullOrEmpty(x.Status)).WithMessage("Невідомий статус КП.");
        RuleFor(x => x.ResponsibleUserId).GreaterThan(0).When(x => x.ResponsibleUserId is not null);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId is not null);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
