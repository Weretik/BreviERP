using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Finalize;
using Crm.Application.Files.GetContent;
using Crm.Application.Files.Upload;
using IntegrationTests.Crm.Files.Support;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.Files;

[NonParallelizable]
public sealed class PrivateFileApiTests
{
    [Test]
    public async Task Anonymous_and_user_roles_cannot_read_or_upload_private_files()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync($"/api/crm/files/{Guid.NewGuid()}/content")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var user = CrmApiTestHost.AsRole(host.Client, "User", "22");
        using var form = Form(Guid.NewGuid());
        Assert.That((await host.Client.PostAsync("/api/crm/files", form)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Upload_returns_staged_metadata_and_rejects_posted_owner_or_unknown_selection_fields()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var root = Guid.NewGuid();
        var file = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<UploadPrivateFileCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PrivateFileView>>(Result.Success(new PrivateFileView(file, root, "application/pdf",
                FileLifecycleTestData.Pdf.Length, new string('A', 64), "Uploaded", DateTimeOffset.UtcNow.AddDays(1), false, false, false))));
        using var form = Form(root);
        var response = await host.Client.PostAsync("/api/crm/files", form);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That((await response.Content.ReadFromJsonAsync<PrivateFileView>())!.IsFinalized, Is.False);
        host.Sender.Verify(x => x.Send(It.Is<UploadPrivateFileCommand>(command => command.DraftId == root
            && command.Actor.UserId == 17 && !command.Actor.IsAdmin), It.IsAny<CancellationToken>()), Times.Once);
        using var forged = Form(root);
        forged.Add(new StringContent("999"), "ownerId");
        Assert.That((await host.Client.PostAsync("/api/crm/files", forged)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var json = new { requestId = Guid.NewGuid(), expectedRevision = 1, files = new[] { new { fileId = file,
            showInQuote = true, production = false, ownerId = 999 } } };
        Assert.That((await host.Client.PutAsJsonAsync($"/api/crm/custom-sewing/{root}/files", json)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<SetCustomFilesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Content_is_private_nosniff_and_foreign_ids_are_not_found()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "18");
        var file = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<GetPrivateFileContentQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PrivateFileContent>>(Result.NotFound()));
        Assert.That((await host.Client.GetAsync($"/api/crm/files/{file}/content")).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        host.Sender.Setup(x => x.Send(It.IsAny<GetPrivateFileContentQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PrivateFileContent>>(Result.Success(new PrivateFileContent(
                new(file, Guid.NewGuid(), "application/pdf", FileLifecycleTestData.Pdf.Length, new string('A', 64)), FileLifecycleTestData.Pdf))));
        var content = await host.Client.GetAsync($"/api/crm/files/{file}/content");
        Assert.That(content.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/pdf"));
        Assert.That(content.Headers.CacheControl!.NoStore, Is.True);
        Assert.That(content.Headers.CacheControl.Private, Is.True);
        Assert.That(content.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
        Assert.That(await content.Content.ReadAsByteArrayAsync(), Is.EqualTo(FileLifecycleTestData.Pdf));
    }

    [TestCase("unsupported_file_type", HttpStatusCode.UnsupportedMediaType)]
    [TestCase("file_too_large", HttpStatusCode.RequestEntityTooLarge)]
    [TestCase("private_file_storage_unavailable", HttpStatusCode.ServiceUnavailable)]
    public async Task Upload_maps_typed_file_errors_to_documented_status(string code, HttpStatusCode status)
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var manager = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        host.Sender.Setup(x => x.Send(It.IsAny<UploadPrivateFileCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PrivateFileView>>(Result.Invalid(new ValidationError("file", code))));
        using var form = Form(Guid.NewGuid());
        Assert.That((await host.Client.PostAsync("/api/crm/files", form)).StatusCode, Is.EqualTo(status));
    }

    private static MultipartFormDataContent Form(Guid root)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(root.ToString()), "draftId");
        form.Add(new StringContent(Guid.NewGuid().ToString()), "requestId");
        var content = new ByteArrayContent(FileLifecycleTestData.Pdf);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(content, "file", "synthetic.pdf");
        return form;
    }
}
