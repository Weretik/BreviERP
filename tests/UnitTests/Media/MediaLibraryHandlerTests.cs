using Ardalis.Result;
using Media.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Media.Application.Features.Files.Commands.DeleteMediaFile;
using Media.Application.Features.Files.Commands.MoveMediaFiles;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Queries.SearchMediaFiles;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Commands.DeleteMediaFolder;
using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Media.Application.Features.Folders.Queries.GetMediaFolders;
using Media.Application.Features.Folders.Shared;

namespace UnitTests.Media;

public sealed class MediaLibraryHandlerTests
{
    [Test]
    public async Task Folder_names_are_unique_among_siblings_ignoring_case()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");
        await media.FolderAsync("Jackets", products);

        var duplicate = await new CreateMediaFolderHandler(media.Db, media.Time)
            .Handle(new CreateMediaFolderCommand(new MediaFolderInput("  jackets ", products)), CancellationToken.None);
        var otherParent = await new CreateMediaFolderHandler(media.Db, media.Time)
            .Handle(new CreateMediaFolderCommand(new MediaFolderInput("Jackets", null)), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(otherParent.IsSuccess, Is.True);
        });
    }

    [Test]
    public async Task Folder_cannot_be_moved_into_its_subfolder()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");
        var jackets = await media.FolderAsync("Jackets", products);

        var result = await new UpdateMediaFolderHandler(media.Db, media.Time)
            .Handle(new UpdateMediaFolderCommand(products, new MediaFolderInput("Products", jackets)), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Only_an_empty_folder_can_be_deleted()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");
        await media.UploadAsync("coat.jpg", products);
        var empty = await media.FolderAsync("Empty");
        var handler = new DeleteMediaFolderHandler(media.Db);

        var blocked = await handler.Handle(new DeleteMediaFolderCommand(products), CancellationToken.None);
        var deleted = await handler.Handle(new DeleteMediaFolderCommand(empty), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(blocked.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(deleted.IsSuccess, Is.True);
        });
    }

    [Test]
    public async Task Upload_stores_the_file_on_the_cdn_first_and_counts_it_in_its_folder()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");

        var file = await media.UploadAsync("C:\\photos\\coat red.jpg", products);
        var folders = await new GetMediaFoldersHandler(media.Db).Handle(new GetMediaFoldersQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(file.FileName, Is.EqualTo("coat red.jpg"));
            Assert.That(file.FolderId, Is.EqualTo(products));
            Assert.That(file.PublicUrl, Does.StartWith("https://cdn.example.test/library/original/").And.EndWith(".jpg"));
            Assert.That(media.Storage.Files, Has.Count.EqualTo(1));
            Assert.That(folders.Value.Single().FileCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Upload_into_a_missing_folder_does_not_touch_the_cdn()
    {
        using var media = new MediaTestHarness();
        await using var content = new MemoryStream(MediaTestHarness.Jpeg);

        var result = await media.UploadHandler()
            .Handle(new UploadMediaFileCommand(content, "a.jpg", "image/jpeg", 12, 999), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(media.Storage.Files, Is.Empty);
        });
    }

    [Test]
    public async Task Search_finds_photos_by_name_in_a_folder_with_its_subfolders()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");
        var jackets = await media.FolderAsync("Jackets", products);
        var site = await media.FolderAsync("Site");
        await media.UploadAsync("Coat-blue.jpg", jackets);
        await media.UploadAsync("coat-red.jpg", products);
        await media.UploadAsync("coat-site.jpg", site);
        await media.UploadAsync("gloves.jpg", products);
        var handler = new SearchMediaFilesHandler(media.Db);

        var inBranch = await handler.Handle(new SearchMediaFilesQuery(products, true, "COAT", 1, 48), CancellationToken.None);
        var onlyFolder = await handler.Handle(new SearchMediaFilesQuery(products, false, "coat", 1, 48), CancellationToken.None);
        var everywhere = await handler.Handle(new SearchMediaFilesQuery(null, true, "coat", 1, 2), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(inBranch.Value.Items.Select(x => x.FileName), Is.EquivalentTo(new[] { "Coat-blue.jpg", "coat-red.jpg" }));
            Assert.That(onlyFolder.Value.Items.Select(x => x.FileName), Is.EqualTo(new[] { "coat-red.jpg" }));
            Assert.That(everywhere.Value.Total, Is.EqualTo(3));
            Assert.That(everywhere.Value.Items, Has.Count.EqualTo(2));
            Assert.That(everywhere.Value.Items[0].FileName, Is.EqualTo("coat-site.jpg"), "newest first");
        });
    }

    [Test]
    public async Task Moving_photos_keeps_their_cdn_urls()
    {
        using var media = new MediaTestHarness();
        var products = await media.FolderAsync("Products");
        var site = await media.FolderAsync("Site");
        var file = await media.UploadAsync("coat.jpg", products);

        var result = await new MoveMediaFilesHandler(media.Db, media.Time)
            .Handle(new MoveMediaFilesCommand(new MoveMediaFilesInput([file.Id], site)), CancellationToken.None);
        var moved = await media.Db.Files.AsNoTracking().SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(moved.FolderId, Is.EqualTo(site));
            Assert.That(moved.PublicUrl, Is.EqualTo(file.PublicUrl));
        });
    }

    [Test]
    public async Task Photo_used_by_a_product_is_not_deleted()
    {
        using var media = new MediaTestHarness();
        var used = await media.UploadAsync("used.jpg", null);
        var free = await media.UploadAsync("free.jpg", null);
        media.Usages.Usages.Add(new MediaUsage(used.Id, "Товари", 7, "Куртка"));
        var handler = new DeleteMediaFileHandler(media.Db, media.Storage, media.Usages);

        var blocked = await handler.Handle(new DeleteMediaFileCommand(used.Id), CancellationToken.None);
        var deleted = await handler.Handle(new DeleteMediaFileCommand(free.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(blocked.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(blocked.Errors.Single(), Does.Contain("Куртка"));
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(media.Storage.Deleted, Has.Count.EqualTo(1));
            Assert.That(media.Db.Files.Select(x => x.Id), Is.EqualTo(new[] { used.Id }));
        });
    }

    [Test]
    public async Task Upload_validator_rejects_a_fake_image_and_a_too_big_file()
    {
        var validator = new UploadMediaFileValidator(new MediaUploadSettings { MaxFileSizeBytes = 100, BaseFolder = "/x" });
        await using var notJpeg = new MemoryStream("not an image"u8.ToArray());
        await using var jpeg = new MemoryStream(MediaTestHarness.Jpeg);

        var fake = await validator.ValidateAsync(new UploadMediaFileCommand(notJpeg, "a.jpg", "image/jpeg", 12, null));
        var big = await validator.ValidateAsync(new UploadMediaFileCommand(jpeg, "a.jpg", "image/jpeg", 101, null));
        var gif = await validator.ValidateAsync(new UploadMediaFileCommand(jpeg, "a.gif", "image/gif", 12, null));

        Assert.Multiple(() =>
        {
            Assert.That(fake.IsValid, Is.False);
            Assert.That(big.Errors.Select(x => x.ErrorMessage), Has.Some.Contains("не більше"));
            Assert.That(gif.IsValid, Is.False);
        });
    }
}
