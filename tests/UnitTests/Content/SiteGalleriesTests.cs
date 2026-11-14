using Ardalis.Result;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using FluentValidation.TestHelper;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;

namespace UnitTests.Content;

/// <summary>«Контент → Наші роботи», «Контент → Сертифікати» and their pictures on the public site (CNT-011).</summary>
public sealed class SiteGalleriesTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static SiteGalleryLegacyPhoto File(string key, string name) =>
        new($"/assets/{key}/{name}.webp", $"/assets/{key}/{name}-thumb.webp", 400, 300);

    /// <summary>The list the migration seeds: pictures the site keeps as its own files.</summary>
    private static void Seed(ContentTestHarness h, string key, params string[] names)
    {
        using var db = h.Open();
        var gallery = SiteGallery.Create(key, "Дані сайту", h.Time.GetUtcNow()).Value;
        gallery.Replace(names.Select(name => new SiteGalleryItem(null, File(key, name),
            new SiteText($"Документ {name}", ""), null, ["ДСТУ EN ISO 13688"])).ToArray(), "Дані сайту", h.Time.GetUtcNow());
        db.SiteGalleries.Add(gallery);
        db.SaveChanges();
    }

    private static SiteGalleryItemInput Photo(int id, string? title = null) =>
        new(id, null, title is null ? null : new GalleryText(title, null), null, null);

    private static SiteGalleryItemInput Legacy(string key, string name) =>
        new(null, File(key, name).Path, new GalleryText($"Документ {name}", null), null, ["ДСТУ EN ISO 13688"]);

    [Test]
    public async Task Works_are_saved_as_a_whole_list_with_a_revision()
    {
        using var h = new ContentTestHarness();
        h.AddPhoto(1);
        h.AddPhoto(2, withThumbnail: false);

        var empty = (await h.Gallery().Handle(new GetSiteGalleryQuery(SiteGallery.Works), None)).Value;
        var saved = await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Works, 0,
            [Photo(2, "  Костюм  для кухаря "), Photo(1)], ContentTestHarness.Editor), None);
        var stale = await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Works, 0, [Photo(1)],
            ContentTestHarness.Editor), None);
        var read = (await h.Gallery().Handle(new GetSiteGalleryQuery(SiteGallery.Works), None)).Value;
        var unknown = await h.Gallery().Handle(new GetSiteGalleryQuery("blog"), None);

        Assert.Multiple(() =>
        {
            Assert.That(empty.Revision, Is.Zero);
            Assert.That(empty.Items, Is.Empty);
            Assert.That(saved.Value.Revision, Is.EqualTo(1));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(read.Items.Select(x => x.PhotoId), Is.EqualTo(new int?[] { 2, 1 }));
            Assert.That(read.Items[0].Title, Is.EqualTo(new GalleryText("Костюм для кухаря", "")));
            Assert.That(read.Items[0].ThumbnailUrl, Is.EqualTo("https://cdn.test/photos/2.jpg"));
            Assert.That(read.Items[1].ThumbnailUrl, Is.EqualTo("https://cdn.test/photos/1-small.webp"));
            Assert.That(read.UpdatedBy, Is.EqualTo(ContentTestHarness.Editor));
            Assert.That(unknown.Status, Is.EqualTo(ResultStatus.NotFound));
        });
    }

    [Test]
    public async Task A_site_file_can_stay_but_not_be_added_and_a_new_photo_must_exist()
    {
        using var h = new ContentTestHarness();
        Seed(h, SiteGallery.Certificates, "a", "b");
        h.AddPhoto(7);

        var unknownFile = await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Certificates, 1,
            [Legacy(SiteGallery.Certificates, "c")], ContentTestHarness.Editor), None);
        var missing = await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Certificates, 1,
            [Photo(99, "Сертифікат")], ContentTestHarness.Editor), None);
        var saved = await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Certificates, 1,
            [Legacy(SiteGallery.Certificates, "b"), Photo(7, "Декларація")], ContentTestHarness.Editor), None);

        Assert.Multiple(() =>
        {
            Assert.That(unknownFile.ValidationErrors.Single().ErrorMessage, Is.EqualTo(SiteGalleryMessages.UnknownFile));
            Assert.That(missing.ValidationErrors.Single().ErrorMessage, Is.EqualTo(SiteGalleryMessages.MissingPhoto));
            Assert.That(saved.Value.Revision, Is.EqualTo(2));
            Assert.That(saved.Value.LegacyCount, Is.EqualTo(1));
            Assert.That(saved.Value.Items[0].Url, Is.EqualTo(SiteGalleryFiles.SiteOrigin + "/assets/certificates/b.webp"));
            Assert.That(saved.Value.Items[0].Width, Is.EqualTo(400));
            Assert.That(saved.Value.Items[0].Standards, Is.EqualTo(new[] { "ДСТУ EN ISO 13688" }));
            Assert.That(saved.Value.Items[1].PhotoId, Is.EqualTo(7));
        });
    }

    [Test]
    public void Validator_names_each_wrong_list()
    {
        var validator = new SaveSiteGalleryValidator();
        SaveSiteGalleryCommand Command(string key, params SiteGalleryItemInput[] items) =>
            new(key, 0, items, ContentTestHarness.Editor);

        var noPicture = validator.TestValidate(Command(SiteGallery.Works, new SiteGalleryItemInput(null, null, null, null, null)));
        var badFile = validator.TestValidate(Command(SiteGallery.Works, new SiteGalleryItemInput(null, "/etc/passwd", null, null, null)));
        var twice = validator.TestValidate(Command(SiteGallery.Works, Photo(1), Photo(1)));
        var noTitle = validator.TestValidate(Command(SiteGallery.Certificates, Photo(1, "  ")));
        var longNote = validator.TestValidate(Command(SiteGallery.Works,
            new SiteGalleryItemInput(1, null, null, new GalleryText(new string('а', SiteGallery.NoteMaxLength + 1), null), null)));
        var standards = validator.TestValidate(Command(SiteGallery.Works,
            new SiteGalleryItemInput(1, null, null, null, Enumerable.Repeat("ДСТУ", SiteGallery.MaxStandards + 1).ToArray())));
        var tooMany = validator.TestValidate(Command(SiteGallery.Certificates,
            Enumerable.Range(1, SiteGallery.MaxCertificates + 1).Select(id => Photo(id, "Документ")).ToArray()));
        var good = validator.TestValidate(Command(SiteGallery.Works, Photo(1), Legacy(SiteGallery.Works, "kuhar-1")));

        Assert.Multiple(() =>
        {
            Assert.That(noPicture.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.NoPicture));
            Assert.That(badFile.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.UnknownFile));
            Assert.That(twice.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.Twice));
            Assert.That(noTitle.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.NoTitle));
            Assert.That(longNote.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.NoteTooLong));
            Assert.That(standards.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.Standards));
            Assert.That(tooMany.Errors.Select(x => x.ErrorMessage), Does.Contain(SiteGalleryMessages.TooManyCertificates));
            Assert.That(good.IsValid, Is.True);
        });
    }

    [Test]
    public async Task Transfer_moves_site_files_to_the_library_in_batches_and_keeps_failed_ones()
    {
        using var h = new ContentTestHarness();
        var names = Enumerable.Range(1, 12).Select(i => $"work-{i}").ToArray();
        Seed(h, SiteGallery.Works, names);
        h.OldSitePhotos.Imported[SiteGalleryFiles.SourceKey("/assets/works/work-1.webp")] = 42;
        h.AddPhoto(42);
        h.OldSitePhotos.Broken.Add(SiteGalleryFiles.Url("/assets/works/work-2.webp").AbsoluteUri);

        var first = (await h.TransferGallery().Handle(new TransferSiteGalleryCommand(SiteGallery.Works, ContentTestHarness.Editor), None)).Value;
        var second = (await h.TransferGallery().Handle(new TransferSiteGalleryCommand(SiteGallery.Works, ContentTestHarness.Editor), None)).Value;
        var read = (await h.Gallery().Handle(new GetSiteGalleryQuery(SiteGallery.Works), None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(first.Moved, Is.EqualTo(9));
            Assert.That(first.Left, Is.EqualTo(3));
            Assert.That(first.Problems.Single(), Does.StartWith($"{TransferSiteGalleryHandler.Failed} /assets/works/work-2.webp"));
            Assert.That(h.OldSitePhotos.Downloads.Select(x => x.Folder).Distinct(), Is.EqualTo(new[] { "Сайт/Наші роботи" }));
            Assert.That(h.OldSitePhotos.Downloads.Any(x => x.Key.EndsWith("work-1.webp")), Is.False);
            Assert.That(second.Moved, Is.EqualTo(2));
            Assert.That(second.Left, Is.EqualTo(1));
            Assert.That(read.LegacyCount, Is.EqualTo(1));
            Assert.That(read.Items[0].PhotoId, Is.EqualTo(42));
            Assert.That(read.Items[0].Title.Uk, Is.EqualTo("Документ work-1"));
            Assert.That(read.Items[1].LegacyPath, Is.EqualTo("/assets/works/work-2.webp"));
            Assert.That(read.Revision, Is.GreaterThan(1));
        });
    }

    [Test]
    public async Task Site_gets_files_by_their_own_path_photos_by_cdn_and_skips_deleted_photos()
    {
        using var h = new ContentTestHarness();
        Seed(h, SiteGallery.Certificates, "a");
        h.AddPhoto(5);
        h.AddPhoto(6);
        await h.SaveGallery().Handle(new SaveSiteGalleryCommand(SiteGallery.Works, 0, [Photo(5, "Халат"), Photo(6)],
            ContentTestHarness.Editor), None);
        h.Media.Photos.RemoveAll(x => x.Id == 6);

        var site = await h.PublicGalleries().GetAsync(None);

        Assert.Multiple(() =>
        {
            Assert.That(site.Works.Select(x => x.Url), Is.EqualTo(new[] { "https://cdn.test/photos/5.jpg" }));
            Assert.That(site.Works[0].ThumbnailUrl, Is.EqualTo("https://cdn.test/photos/5-small.webp"));
            Assert.That(site.Works[0].Title, Is.EqualTo(new GalleryText("Халат", "")));
            Assert.That(site.Certificates.Single().Url, Is.EqualTo("/assets/certificates/a.webp"));
            Assert.That(site.Certificates.Single().ThumbnailUrl, Is.EqualTo("/assets/certificates/a-thumb.webp"));
            Assert.That(site.Certificates.Single().Standards, Is.EqualTo(new[] { "ДСТУ EN ISO 13688" }));
        });
    }
}
