namespace Crm.Application.Quotes.Photos;

/// <summary>Stores a photo uploaded in «Змінити фото» in the CDN folder of КП photos; the Host uses the Media library.</summary>
public interface IQuotePhotoStorage
{
    /// <summary>The public URL of the stored photo, or the reason the library refused it.</summary>
    Task<Result<string>> StoreAsync(Stream content, string fileName, string contentType, long sizeInBytes,
        CancellationToken cancellationToken);
}
