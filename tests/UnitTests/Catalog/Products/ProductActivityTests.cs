using BuildingBlocks.Domain.Identifiers.Products;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Catalog.Application.Features.Activity.EventHandlers;
using Catalog.Domain.ProductCategories;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests.Catalog.Products;

/// <summary>Products and categories read as «Журнал дій» entries (ACT-001): one entry per command.</summary>
public sealed class ProductActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private static Product NewProduct() => Product.Create(new ProductId(101), "Фартух", "Фартук",
        ProductSlug.Create("fartukh").Value, ProductType.Sewing, Now).Value;

    private static IReadOnlyCollection<IDomainEvent> Events(IHasDomainEvents source) => source.DomainEvents;

    [Test]
    public void A_new_product_with_its_first_details_is_one_created_entry()
    {
        var product = NewProduct();
        product.SetDescriptions("Опис", "Описание", Now);

        var entry = new ProductActivity().Describe(Events(product).Single());

        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectType, Is.EqualTo("product"));
        Assert.That(entry.SubjectTitle, Is.EqualTo("Фартух"));
    }

    [Test]
    public void One_command_that_changes_several_parts_is_one_entry_listing_them()
    {
        var product = NewProduct();
        ((IHasDomainEvents)product).ClearDomainEvents();

        product.Update("Фартух кухарський", "Фартук повара", ProductSlug.Create("fartukh").Value, ProductType.Sewing, Now);
        product.SetDescriptions("Новий опис", "Новое описание", Now);
        product.AddCategory(ProductCategoryId.Create(5), Now);

        var entry = new ProductActivity().Describe(Events(product).Single());
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Changed));
        Assert.That(entry.Summary, Is.EqualTo("Змінено: категорії, назва і тип, опис"));
        var name = entry.Changes.Single();
        Assert.That((name.Field, name.From, name.To), Is.EqualTo(("Назва", "Фартух", "Фартух кухарський")));
    }

    [Test]
    public void Deleting_a_product_is_listed()
    {
        var product = NewProduct();
        ((IHasDomainEvents)product).ClearDomainEvents();

        product.MarkDeleted();

        Assert.That(new ProductActivity().Describe(Events(product).Single())!.Action,
            Is.EqualTo(ActivityActions.Deleted));
    }

    [Test]
    public void Switching_a_category_off_lists_it_as_a_field()
    {
        var category = ProductCategory.Create(ProductCategoryId.Create(5), "Фартухи", "Фартуки", "fartukhy").Value;
        ((IHasDomainEvents)category).ClearDomainEvents();

        category.Update("Фартухи", "Фартуки", "fartukhy", 0, false);

        var entry = new ProductCategoryActivity().Describe(Events(category).Single());
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Changed));
        var active = entry.Changes.Single();
        Assert.That((active.Field, active.From, active.To), Is.EqualTo(("Активна", "так", "ні")));
    }
}
