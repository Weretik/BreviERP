using Crm.Domain.Quotes;

namespace Crm.Application.Features.Quotes.Queries.SearchQuoteCatalog;

public sealed class SearchQuoteCatalogValidator : AbstractValidator<SearchQuoteCatalogQuery>
{
    public const int MaxPageSize = 50;

    public SearchQuoteCatalogValidator()
    {
        RuleFor(x => x.Kind).Must(x => x is nameof(QuoteItemKind.Sewing) or nameof(QuoteItemKind.Ppe))
            .WithMessage("Оберіть швейну продукцію або ЗІЗ.");
        RuleFor(x => x.Search).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Season)
            .Must(x => Enum.TryParse<QuoteIntroSeason>(x, false, out var season) && Enum.IsDefined(season))
            .WithMessage("Невідомий сезон.").When(x => x.Season is not null);
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
