using ActivityLog.Contracts.Activity;
using Catalog.Domain.ProductCategories;

namespace Catalog.Application.Features.Activity.EventHandlers;

/// <summary>How catalogue categories read in «Журнал дій» (ACT-001): created, changed (name and «Активна»), moved,
/// deleted.</summary>
public sealed class ProductCategoryActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent)
    {
        if (domainEvent is not ProductCategoryRecorded e) return null;
        var category = e.Category;
        var changes = new ActivityChanges();
        if (e.Action == ProductCategoryAction.Changed)
        {
            changes.Add("Назва", e.NameBefore, category.Name);
            if (e.ActiveBefore is { } before) changes.Add("Активна", before, category.IsActive);
        }

        var (action, summary) = e.Action switch
        {
            ProductCategoryAction.Created => (ActivityActions.Created, (string?)null),
            ProductCategoryAction.Deleted => (ActivityActions.Deleted, null),
            ProductCategoryAction.Moved => (ActivityActions.Changed, "Перенесено в інший розділ"),
            _ => (ActivityActions.Changed, null),
        };
        return new ActivityDescription(action, "product-category", "Категорія товарів", category.Id.Value.ToString(),
            category.Name, summary, changes.Items, "Catalog");
    }
}
