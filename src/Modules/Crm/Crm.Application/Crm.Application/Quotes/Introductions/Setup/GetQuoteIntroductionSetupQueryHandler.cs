using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Settings.Reading;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Introductions.Setup;

public sealed class GetQuoteIntroductionSetupQueryHandler(IQuoteStore store, IQuoteCatalogReader catalog)
    : IQueryHandler<GetQuoteIntroductionSetupQuery, Result<QuoteIntroductionSetupResponse>>
{
    public async ValueTask<Result<QuoteIntroductionSetupResponse>> Handle(GetQuoteIntroductionSetupQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await store.SettingsContentAsync(cancellationToken);
        var summer = await MainModelAsync(settings.Introduction.MainCode(QuoteIntroSeason.Summer), cancellationToken);
        var winter = await MainModelAsync(settings.Introduction.MainCode(QuoteIntroSeason.Winter), cancellationToken);
        var letters = settings.LetterVariants.Select(x => new QuoteIntroLetterResponse(x.Name, x.Subject)).ToArray();
        return Result.Success(new QuoteIntroductionSetupResponse(summer, winter, letters));
    }

    private async Task<QuoteIntroMainModelResponse> MainModelAsync(string code, CancellationToken cancellationToken)
    {
        var found = await catalog.SearchAsync(QuoteItemKind.Sewing, code, null, 1, 20, cancellationToken);
        var match = found.Items.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            return new QuoteIntroMainModelResponse(code, null,
                $"Модель {code} не знайдено в каталозі. Перевірте код у «Налаштування КП».");

        var products = await catalog.GetAsync([match.ProductId], cancellationToken);
        var product = (products.GetValueOrDefault(match.ProductId) ?? match).ToResponse();
        return new QuoteIntroMainModelResponse(code, product, product.Problem);
    }
}
