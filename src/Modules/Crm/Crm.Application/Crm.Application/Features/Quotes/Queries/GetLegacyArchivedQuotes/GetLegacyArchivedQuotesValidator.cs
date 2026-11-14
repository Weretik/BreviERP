
namespace Crm.Application.Features.Quotes.Queries.GetLegacyArchivedQuotes;

public sealed class GetLegacyArchivedQuotesValidator : AbstractValidator<GetLegacyArchivedQuotesQuery>
{
    public GetLegacyArchivedQuotesValidator() => RuleFor(x => x.Actor).NotNull();
}
