using System.Net;
using System.Net.Http.Headers;
using Ardalis.Result;
using HostApiTests.TestSupport;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Shared;

namespace HostApiTests;

/// <summary>
/// Multipart uploads bind on Minimal API as they did on MVC: the file and the fields by name, no antiforgery token,
/// and the endpoint's own 400 when the file is missing.
/// </summary>
public sealed class FormUploadBindingTests
{
    [Test]
    public async Task Media_upload_binds_the_file_and_the_folder()
    {
        var (sender, recorder) = RecordingSender.Create(Result<MediaFileResponse>.NotFound());
        await using var host = await ApiTestHost.CreateAsync(sender);
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", "Admin");
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2, 3]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(file, "file", "photo.jpg");
        form.Add(new StringContent("5"), "folderId");

        var response = await host.Client.PostAsync("/api/media/files", form);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var command = recorder.Messages.OfType<UploadMediaFileCommand>().Single();
            Assert.That((command.FileName, command.ContentType, command.SizeInBytes, command.FolderId),
                Is.EqualTo(("photo.jpg", "image/jpeg", 3L, (int?)5)));
        });
    }

    [Test]
    public async Task Media_upload_without_a_file_is_400()
    {
        var (sender, recorder) = RecordingSender.Create(null);
        await using var host = await ApiTestHost.CreateAsync(sender);
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", "Admin");
        using var form = new MultipartFormDataContent { { new StringContent("5"), "folderId" } };

        var response = await host.Client.PostAsync("/api/media/files", form);

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("File is required."));
            Assert.That(recorder.Messages, Is.Empty);
        });
    }
}
