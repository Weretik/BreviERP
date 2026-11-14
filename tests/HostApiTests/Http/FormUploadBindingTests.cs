using System.Net;
using System.Net.Http.Headers;
using Ardalis.Result;
using HostApiTests.TestSupport;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Shared;
using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;
using Crm.Application.Features.SiteRequests.Shared;

namespace HostApiTests.Http;

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

    [Test]
    public async Task Public_site_request_binds_the_form_fields_and_the_attachment()
    {
        var (sender, recorder) = RecordingSender.Create(Result<SiteRequestReceivedResponse>.Success(new(7)));
        await using var host = await ApiTestHost.CreateAsync(sender);
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent([1, 2]);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "attachment", "brief.pdf");
        form.Add(new StringContent("0504698675"), "phone");
        form.Add(new StringContent("uk"), "locale");

        var response = await host.Client.PostAsync("/api/public/site-requests", form);

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("{\"id\":7}"));
            var command = recorder.Messages.OfType<SubmitSiteRequestCommand>().Single();
            Assert.That((command.Phone, command.Locale, command.Workwear, command.File?.FileName, command.File?.Length),
                Is.EqualTo(("0504698675", "uk", (string?)null, "brief.pdf", (long?)2)));
        });
    }

    [Test]
    public async Task A_missing_array_query_binds_as_an_empty_list_as_on_MVC()
    {
        var (sender, recorder) = RecordingSender.Create(
            Result<IReadOnlyList<QuoteFabricFactsResponse>>.Success([]));
        await using var host = await ApiTestHost.CreateAsync(sender);
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", "Admin");

        var none = await host.Client.GetAsync("/api/crm/quote-lookups/fabrics");
        var two = await host.Client.GetAsync("/api/crm/quote-lookups/fabrics?ids=3&ids=5");

        Assert.Multiple(() =>
        {
            Assert.That((none.StatusCode, two.StatusCode), Is.EqualTo((HttpStatusCode.OK, HttpStatusCode.OK)));
            Assert.That(recorder.Messages.OfType<GetQuoteFabricFactsQuery>().Select(x => string.Join(",", x.FabricIds)),
                Is.EqualTo(new[] { "", "3,5" }));
        });
    }
}
