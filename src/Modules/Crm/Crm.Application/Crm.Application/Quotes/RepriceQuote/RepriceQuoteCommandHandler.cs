using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.RepriceQuote;

public sealed class RepriceQuoteCommandHandler(IQuoteStore store, IQuoteCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<RepriceQuoteCommand, Result<QuoteRepriceReport>>
{
    public async ValueTask<Result<QuoteRepriceReport>> Handle(RepriceQuoteCommand command,
        CancellationToken cancellationToken)
    {
        var quote = await store.LoadAsync(command.Id, cancellationToken);
        if (quote is null || !command.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);
        if (quote.Version != command.Version) return Result.Conflict(QuoteMessages.Stale);

        var productIds = quote.Items.Where(x => x.ProductId is not null).Select(x => x.ProductId!.Value).Distinct()
            .ToArray();
        var products = productIds.Length == 0
            ? new Dictionary<int, QuoteCatalogProduct>()
            : await catalog.GetAsync(productIds, cancellationToken);

        var now = clock.GetUtcNow();
        var changes = new List<QuoteRepriceChange>();
        var unavailable = new List<QuoteRepriceProblem>();
        foreach (var item in quote.Items.Where(x => x.ProductId is not null).OrderBy(x => x.Position))
        {
            products.TryGetValue(item.ProductId!.Value, out var product);
            foreach (var option in item.Options.OrderBy(x => x.Position))
            {
                var current = CurrentPrices(item, option, product, out var reason);
                if (current is null)
                {
                    unavailable.Add(new QuoteRepriceProblem(option.Id, item.Name, option.Title, reason));
                    continue;
                }

                if (option.Retail.Source == current.Retail && option.Wholesale.Source == current.Wholesale &&
                    option.Large.Source == current.Large) continue;

                var before = option.Sources();
                if (command.Apply) option.ReplaceSourcePrices(current.Retail, current.Wholesale, current.Large, now);
                changes.Add(new QuoteRepriceChange(option.Id, item.Name, option.Title, before,
                    new QuoteTierPricesResponse(current.Retail, current.Wholesale, current.Large)));
            }
        }

        var applied = command.Apply && changes.Count > 0;
        if (applied)
        {
            quote.Touch(now);
            try
            {
                await store.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Conflict(QuoteMessages.Stale);
            }
        }

        return Result.Success(new QuoteRepriceReport(applied, quote.Version, changes, unavailable));
    }

    private static QuoteTierPrices? CurrentPrices(QuoteItem item, QuoteItemOption option, QuoteCatalogProduct? product,
        out string reason)
    {
        reason = "Товар видалено з каталогу.";
        if (product is null) return null;
        if (!product.IsActive)
        {
            reason = "Товар вимкнений у каталозі.";
            return null;
        }

        if (item.Kind == QuoteItemKind.Ppe)
        {
            reason = Problems(product.Problems, "Немає ціни в каталозі.");
            return product.Prices;
        }

        var variant = product.Variants.FirstOrDefault(x => x.ArticleId == option.ArticleId);
        if (variant is null || !variant.IsActive)
        {
            reason = "Варіант тканини видалено або вимкнено.";
            return null;
        }

        reason = Problems(variant.Problems, "Немає опублікованої ціни.");
        return variant.Prices;
    }

    private static string Problems(IReadOnlyList<string> problems, string fallback) =>
        problems.Count == 0 ? fallback : string.Join(", ", problems);
}
