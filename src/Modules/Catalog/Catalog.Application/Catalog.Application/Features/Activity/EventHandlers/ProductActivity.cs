using ActivityLog.Contracts.Activity;
using Catalog.Domain.Products.Activity;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Activity.EventHandlers;

/// <summary>
/// How products read in «Журнал дій» (ACT-001): created, deleted, or changed with the name and «Активний» as fields
/// and the touched parts of the card («фото», «склад», …) in the summary. Imports are not listed.
/// </summary>
public sealed class ProductActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent)
    {
        if (domainEvent is not ProductRecorded e) return null;
        var product = e.Product;
        var action = e.Action switch
        {
            ProductAction.Created => ActivityActions.Created,
            ProductAction.Deleted => ActivityActions.Deleted,
            _ => ActivityActions.Changed,
        };
        var changes = new ActivityChanges();
        string? summary = null;
        if (e.Action == ProductAction.Changed)
        {
            changes.Add("Назва", e.NameBefore, product.Name);
            if (e.ActiveBefore is { } before && product.CatalogProfile is { } profile)
                changes.Add("Активний", before, profile.IsActive);
            if (e.Parts.Count > 0) summary = "Змінено: " + string.Join(", ", e.Parts);
            else if (!changes.Any) return null; // a refused change
        }

        return new ActivityDescription(action, "product", "Товар", product.Id.Value.ToString(), Title(product),
            summary, changes.Items, "Catalog");
    }

    /// <summary>«MOD-12 · Фартух» when the product has a code, otherwise the name.</summary>
    private static string Title(ProductEntity product) =>
        string.IsNullOrWhiteSpace(product.Code) ? product.Name : $"{product.Code} · {product.Name}";
}
