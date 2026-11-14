using Catalog.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Catalog.Domain.Products.Sewing.Composition;


namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// Marks <see cref="PricingInputChanges"/> when a save changes a Sewing model's articles or composition (that model)
/// or adds a sewing settings version (every model), so the host publishes the new prices (CAT-012).
/// </summary>
internal sealed class SewingPricingInputChangeInterceptor(PricingInputChanges changes) : SaveChangesInterceptor
{
    private bool _all;
    private readonly HashSet<int> _products = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Collect(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Collect(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Flush();
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Flush();
        return base.SavedChanges(eventData, result);
    }

    private void Collect(DbContext? context)
    {
        _all = false;
        _products.Clear();
        if (context is null)
            return;
        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            switch (entry.Entity)
            {
                case SewingPricingSettingsVersion:
                    _all = true;
                    break;
                case SewingArticle article:
                    _products.Add(article.ProductId.Value);
                    break;
                case SewingMaterialLine material:
                    _products.Add(material.ProductId.Value);
                    break;
                case SewingOperationLine operation:
                    _products.Add(operation.ProductId.Value);
                    break;
            }
        }
    }

    private void Flush()
    {
        if (_all)
            changes.MarkAll();
        foreach (var productId in _products)
            changes.MarkProduct(productId);
        _all = false;
        _products.Clear();
    }
}
