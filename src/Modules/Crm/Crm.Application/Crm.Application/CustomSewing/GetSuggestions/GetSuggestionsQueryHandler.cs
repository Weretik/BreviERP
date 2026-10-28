using Crm.Application.CustomSewing.GetSuggestions.Contracts;
using Ardalis.Result;
using Mediator;
namespace Crm.Application.CustomSewing.GetSuggestions;

public sealed class GetSuggestionsQueryHandler(ISewingSuggestionSource source) : IQueryHandler<GetSuggestionsQuery, Result<SewingSuggestions>>
{
    public async ValueTask<Result<SewingSuggestions>> Handle(GetSuggestionsQuery query, CancellationToken ct)
    {
        if (query.Actor.UserId <= 0 || !LegacySewingSuggestionType.IsSupported(query.ProductType) || LegacySewingSuggestionType.SourceSeason(query.Season) is null)
            return Result.Invalid(new ValidationError("filter", "invalid_suggestion_filter"));
        return Result.Success(SuggestionNormAggregator.Aggregate(query.ProductType, query.Season,
            await source.ReadAsync(query.ProductType, query.Season, ct)));
    }
}
