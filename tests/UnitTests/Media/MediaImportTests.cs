using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Queries.GetMediaFilesBySourceKeys;
using Media.Application.Features.Folders.Commands.EnsureMediaFolderPath;

namespace UnitTests.Media;

/// <summary>Library side of photo imports (CAT-016): source keys and folder paths.</summary>
public sealed class MediaImportTests
{
    [Test]
    public async Task Imported_photo_keeps_its_source_key_and_a_second_upload_with_it_is_a_conflict()
    {
        using var media = new MediaTestHarness();

        var first = await Upload(media, "old-site:catalog/a.jpg");
        var second = await Upload(media, "old-site:catalog/a.jpg");
        var found = await new GetMediaFilesBySourceKeysHandler(media.Db)
            .Handle(new GetMediaFilesBySourceKeysQuery(["old-site:catalog/a.jpg", "old-site:catalog/b.jpg"]), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(media.Storage.Files, Has.Count.EqualTo(1), "the second upload never reaches the CDN");
            Assert.That(found.Value, Is.EqualTo(new Dictionary<string, int> { ["old-site:catalog/a.jpg"] = first.Value.Id }));
        });
    }

    [Test]
    public async Task Folder_path_reuses_existing_folders_ignoring_case_and_creates_the_rest()
    {
        using var media = new MediaTestHarness();
        var root = await media.FolderAsync("Старий сайт");
        var handler = new EnsureMediaFolderPathHandler(media.Db, media.Time);

        var first = await handler.Handle(new EnsureMediaFolderPathCommand(["старий сайт", "costum_leto", "agier"]), CancellationToken.None);
        var again = await handler.Handle(new EnsureMediaFolderPathCommand(["Старий сайт", "costum_leto", "agier"]), CancellationToken.None);

        var folders = await media.Db.Folders.AsNoTracking().ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(again.Value, Is.EqualTo(first.Value));
            Assert.That(folders, Has.Count.EqualTo(3));
            Assert.That(folders.Single(x => x.Name == "costum_leto").ParentId, Is.EqualTo(root));
            Assert.That(folders.Single(x => x.Id == first.Value).Name, Is.EqualTo("agier"));
        });
    }

    [Test]
    public void Folder_path_rejects_slashes_and_too_deep_paths()
    {
        var validator = new EnsureMediaFolderPathValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new EnsureMediaFolderPathCommand(["a/b"])).IsValid, Is.False);
            Assert.That(validator.Validate(new EnsureMediaFolderPathCommand(Enumerable.Repeat("a", 9).ToList())).IsValid, Is.False);
            Assert.That(validator.Validate(new EnsureMediaFolderPathCommand(["Старий сайт", "obuv"])).IsValid, Is.True);
        });
    }

    private static async Task<Result<global::Media.Application.Features.Files.Shared.MediaFileResponse>> Upload(MediaTestHarness media, string sourceKey)
    {
        await using var content = new MemoryStream(MediaTestHarness.Jpeg);
        return await media.UploadHandler().Handle(
            new UploadMediaFileCommand(content, "a.jpg", "image/jpeg", MediaTestHarness.Jpeg.Length, null, sourceKey),
            CancellationToken.None);
    }
}
