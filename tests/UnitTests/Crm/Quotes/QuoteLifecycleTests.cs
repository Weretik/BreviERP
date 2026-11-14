using Ardalis.Result;
using Crm.Application.Features.Quotes.Commands.DeleteQuote;
using Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;
using Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;
using Crm.Application.Features.Quotes.Queries.GetLegacyArchivedQuotes;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Abstractions.Quotes;

namespace UnitTests.Crm.Quotes;

/// <summary>R-33 and R-34: who deletes a КП, and a photo uploaded for one КП item.</summary>
[TestFixture]
public sealed class QuoteLifecycleTests
{
    private static readonly QuoteActor Manager = new(5, false);
    private static readonly QuoteActor Admin = new(2, true);

    [Test]
    public async Task Only_who_may_delete_reaches_the_quote()
    {
        // The store is never touched for a refused actor.
        var delete = await new DeleteQuoteHandler(null!, null!)
            .Handle(new DeleteQuoteCommand(Admin, 1, 1), default);
        var count = await new GetLegacyArchivedQuotesHandler(null!)
            .Handle(new GetLegacyArchivedQuotesQuery(Manager), default);
        var purge = await new PurgeLegacyArchivedQuotesHandler(null!, null!, null!, TimeProvider.System)
            .Handle(new PurgeLegacyArchivedQuotesCommand(Admin), default);

        Assert.Multiple(() =>
        {
            Assert.That(delete.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(count.Status, Is.EqualTo(ResultStatus.Forbidden));
            Assert.That(purge.Status, Is.EqualTo(ResultStatus.Forbidden));
        });
    }

    [Test]
    public async Task An_uploaded_photo_returns_its_cdn_address()
    {
        var storage = new FakePhotoStorage(Result.Success("https://cdn/quotes/a.jpg"));
        var result = await new UploadQuotePhotoHandler(storage)
            .Handle(new UploadQuotePhotoCommand(new MemoryStream([1]), "a.JPG", "IMAGE/JPEG", 1), default);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Url, Is.EqualTo("https://cdn/quotes/a.jpg"));
            Assert.That(storage.ContentType, Is.EqualTo("image/jpeg"));
        });
    }

    [Test]
    public async Task A_refused_photo_says_why()
    {
        var result = await new UploadQuotePhotoHandler(new FakePhotoStorage(Result<string>.Error("Завеликий файл.")))
            .Handle(new UploadQuotePhotoCommand(new MemoryStream([1]), "a.jpg", "image/jpeg", 1), default);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(result.ValidationErrors.Single().ErrorMessage, Is.EqualTo("Завеликий файл."));
        });
    }

    [TestCase("image/png", 10, true)]
    [TestCase("image/gif", 10, false)]
    [TestCase("image/jpeg", 0, false)]
    public void Only_photos_are_uploaded(string contentType, long size, bool valid) =>
        Assert.That(new UploadQuotePhotoValidator()
            .Validate(new UploadQuotePhotoCommand(Stream.Null, "a", contentType, size)).IsValid, Is.EqualTo(valid));

    private sealed class FakePhotoStorage(Result<string> result) : IQuotePhotoStorage
    {
        public string? ContentType { get; private set; }

        public Task<Result<string>> StoreAsync(Stream content, string fileName, string contentType, long sizeInBytes,
            CancellationToken cancellationToken)
        {
            ContentType = contentType;
            return Task.FromResult(result);
        }
    }
}
