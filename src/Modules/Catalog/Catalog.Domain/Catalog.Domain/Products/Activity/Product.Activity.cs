using System.Runtime.CompilerServices;
using BuildingBlocks.Domain.Events;
using Catalog.Domain.Products.Activity;

namespace Catalog.Domain.Products;

/// <summary>Tells «Журнал дій» (ACT-001) what a command did to the product; imports suppress it.</summary>
public partial class Product
{
    /// <summary>Tells «Журнал дій» that the product is being deleted; call it before removing it.</summary>
    public void MarkDeleted() => Raise(new ProductRecorded(this, ProductAction.Deleted, Name, CatalogProfile?.IsActive));

    private void RecordCreated() => Raise(new ProductRecorded(this, ProductAction.Created, Name, null));

    /// <summary>
    /// Every change of the card ends here: stamps the time and adds the part named by the changing method to this
    /// command's event. A change right after the create stays part of «created».
    /// </summary>
    private void MarkAsUpdated(DateTimeOffset now, [CallerMemberName] string method = "")
    {
        base.MarkAsUpdated(now);
        if (Pending() is { Action: ProductAction.Changed } pending && PartOf(method) is { } part) pending.Parts.Add(part);
    }

    /// <summary>
    /// This command's event; the first change makes it with the name and «Активний» as they are now. A method that
    /// changes either calls it before changing them.
    /// </summary>
    private ProductRecorded Pending()
    {
        if (((IHasDomainEvents)this).DomainEvents.OfType<ProductRecorded>().LastOrDefault() is { } pending) return pending;
        pending = new ProductRecorded(this, ProductAction.Changed, Name, CatalogProfile?.IsActive);
        Raise(pending);
        return pending;
    }

    private static string? PartOf(string method) => method switch
    {
        nameof(Update) => "назва і тип",
        nameof(SetDescriptions) or nameof(ReplaceContent) => "опис",
        nameof(ConfigureSewing) or nameof(ConfigurePpe) => "параметри",
        nameof(AddCategory) or nameof(RemoveCategory) or nameof(ReplaceCategories) => "категорії",
        nameof(SetCatalogProfile) or nameof(SetOverview) => "огляд",
        nameof(SetCode) or nameof(SetModelCode) => "код",
        nameof(AddArticle) or nameof(UpdateArticle) or nameof(RemoveArticle) or nameof(ReplaceArticles) => "артикули",
        nameof(ReplaceComposition) or nameof(ReplaceArticleComposition) or nameof(SetArticleFabric) => "склад",
        nameof(SetPpeSupplier) => "постачальник ЗІЗ",
        _ when method.Contains("Photo", StringComparison.Ordinal) => "фото",
        _ => null
    };
}
