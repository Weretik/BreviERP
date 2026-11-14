using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Settings.Reading;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteIntroductionSetup;

public sealed class GetQuoteIntroductionSetupHandler(IQuoteStore store, IQuoteCatalogReader catalog)
    : IQueryHandler<GetQuoteIntroductionSetupQuery, Result<QuoteIntroductionSetupResponse>>
{
    public async ValueTask<Result<QuoteIntroductionSetupResponse>> Handle(GetQuoteIntroductionSetupQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await store.SettingsContentAsync(cancellationToken);
        var summer = await MainModelAsync(settings.Introduction.MainCode(QuoteIntroSeason.Summer), cancellationToken);
        var winter = await MainModelAsync(settings.Introduction.MainCode(QuoteIntroSeason.Winter), cancellationToken);
        var letters = settings.LetterVariants.Select(x => new QuoteIntroLetterResponse(x.Name, x.Subject, x.Body)).ToArray();
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
