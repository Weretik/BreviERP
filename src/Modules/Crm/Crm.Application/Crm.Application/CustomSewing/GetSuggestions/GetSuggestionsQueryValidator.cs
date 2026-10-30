namespace Crm.Application.CustomSewing.GetSuggestions;

public sealed class GetSuggestionsQueryValidator : AbstractValidator<GetSuggestionsQuery>
{
    public GetSuggestionsQueryValidator()
    {
        RuleFor(x => x.Actor.UserId).GreaterThan(0);
        RuleFor(x => x.ProductType).Must(LegacySewingSuggestionType.IsSupported);
        RuleFor(x => x.Season).Must(x => LegacySewingSuggestionType.SourceSeason(x) is not null);
    }
}
