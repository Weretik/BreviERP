using Catalog.Application.Features.Product.OldSitePhotoImport;
using Catalog.Application.Features.Product.Photos.SetPhotos;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

/// <summary>The tab «Фото» of a product card and old-site paths (CAT-016).</summary>
public sealed class ProductPhotoListTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 22, 0, 0, TimeSpan.Zero);

    [Test]
    public void Photo_list_keeps_existing_photos_reorders_them_and_adds_new_ones()
    {
        var product = Product(10, 20);
        var kept = product.Photos.Single(x => x.MediaFileId.Value == 20);
        product.SetPhotoAltText(kept.Id, "alt", Now);

        product.SetPhotoList(Ids(30, 20), null, Now);

        var photos = product.Photos.OrderBy(x => x.SortOrder).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(photos.Select(x => x.MediaFileId.Value), Is.EqualTo(new[] { 30, 20 }));
            Assert.That(photos[1].Id, Is.EqualTo(kept.Id));
            Assert.That(photos[1].Alt, Is.EqualTo("alt"));
            Assert.That(photos[0].Id.Value, Is.EqualTo(3), "a new photo takes the next id");
            Assert.That(photos.Single(x => x.IsMain).MediaFileId.Value, Is.EqualTo(30), "the old main photo was removed");
        });
    }

    [Test]
    public void Photo_list_keeps_the_main_photo_unless_another_is_chosen()
    {
        var product = Product(10, 20);

        product.SetPhotoList(Ids(20, 10), null, Now);
        var keptMain = product.Photos.Single(x => x.IsMain).MediaFileId.Value;
        product.SetPhotoList(Ids(20, 10), MediaFileId.Create(20), Now);

        Assert.Multiple(() =>
        {
            Assert.That(keptMain, Is.EqualTo(10));
            Assert.That(product.Photos.Single(x => x.IsMain).MediaFileId.Value, Is.EqualTo(20));
        });
    }

    [Test]
    public void Photo_list_can_be_emptied()
    {
        var product = Product(10);

        product.SetPhotoList([], null, Now);

        Assert.That(product.Photos, Is.Empty);
    }

    [Test]
    public void Save_rejects_a_repeated_photo_and_a_main_photo_outside_the_list()
    {
        var validator = new SetProductPhotosCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new SetProductPhotosCommand(1, [5, 5], null)).IsValid, Is.False);
            Assert.That(validator.Validate(new SetProductPhotosCommand(1, [5], 6)).IsValid, Is.False);
            Assert.That(validator.Validate(new SetProductPhotosCommand(1, [5, 6], 6)).IsValid, Is.True);
        });
    }

    [Test]
    public void Old_site_paths_map_to_library_folders_and_escaped_addresses()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OldSitePhotoPaths.Folder("catalog/kyrtka zima/novu chatgpt/a.png"),
                Is.EqualTo(new[] { "Старий сайт", "kyrtka zima", "novu chatgpt" }));
            Assert.That(OldSitePhotoPaths.Folder("a.png"), Is.EqualTo(new[] { "Старий сайт" }));
            Assert.That(OldSitePhotoPaths.Url("https://brevi.com.ua/image/", "catalog/kyrtka zima/a b.png").AbsoluteUri,
                Is.EqualTo("https://brevi.com.ua/image/catalog/kyrtka%20zima/a%20b.png"));
            Assert.That(OldSitePhotoPaths.IsSafe("catalog/../secret.png"), Is.False);
            Assert.That(OldSitePhotoPaths.IsSafe("/etc/a.png"), Is.False);
            Assert.That(OldSitePhotoPaths.IsSafe("catalog/a.png"), Is.True);
        });
    }

    private static Product Product(params int[] mediaFileIds)
    {
        var product = Catalog.Domain.Products.Entities.Product.Create(ProductId.Create(1), "Куртка", "Куртка",
            ProductSlug.Create("kurtka"), ProductType.Sewing, Now);
        foreach (var (id, index) in mediaFileIds.Select((id, index) => (id, index)))
            product.AddPhoto(ProductPhotoId.Create(index + 1), MediaFileId.Create(id), Now, sortOrder: index);
        return product;
    }

    private static List<MediaFileId> Ids(params int[] ids) => ids.Select(MediaFileId.Create).ToList();
}
